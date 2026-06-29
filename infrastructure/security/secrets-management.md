# Secrets Management Guide

> **Platform:** Telecom OSS/BSS Platform
> **Last Updated:** 2026-06-21
> **Owner:** Platform Engineering Team

---

## Principles

1. **Never commit secrets to source control.** No exceptions.
2. **Use the right storage** for each environment (local dev vs production).
3. **Rotate secrets regularly** and immediately on compromise.
4. **Least privilege** — each service gets only the secrets it needs.
5. **Audit access** — all secret access is logged.

---

## Environment-Based Approach

| Environment | Storage Mechanism | Access Control |
|------------|-------------------|----------------|
| **Local Development** | `.env` file (gitignored) + User Secrets | Developer's machine only |
| **CI/CD** | GitHub Actions Secrets | Per-repository, scoped to workflows |
| **Staging** | Azure Key Vault / AWS Secrets Manager | RBAC + managed identity |
| **Production** | Azure Key Vault / AWS Secrets Manager | RBAC + managed identity + approval gates |

---

## 1. Local Development

### .env File (Docker Compose)

Use `.env` for Docker Compose local deployments. This file is in `.gitignore` and **never** committed.

```bash
# .env (local, not committed)
POSTGRES_PASSWORD=local_dev_password_change_me
RABBITMQ_PASSWORD=local_dev_password_change_me
REDIS_PASSWORD=local_dev_password_change_me
KEYCLOAK_ADMIN_PASSWORD=local_dev_password_change_me
```

A `.env.example` template is committed with placeholder values:

```bash
# .env.example (committed - placeholder values only)
POSTGRES_PASSWORD=change-me
RABBITMQ_PASSWORD=change-me
```

### .NET User Secrets (ASP.NET Core)

For local development of individual services, use the .NET Secret Manager:

```bash
# Initialize user secrets for the API project
dotnet user-secrets init --project src/Obss.Api

# Set secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost;Database=obss;User Id=sa;Password=local_dev;TrustServerCertificate=true"

dotnet user-secrets set "Auth:JwtSigningKey" \
  "this-is-a-local-dev-key-not-for-production"

# List secrets
dotnet user-secrets list

# Remove a secret
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"

# Clear all secrets
dotnet user-secrets clear
```

**Important:** User Secrets are stored in `~/.microsoft/usersecrets/<guid>/secrets.json` and are never committed.

### Environment Variables (Fallback)

```csharp
// appsettings.json reads from environment variables with fallback
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}

// In Program.cs - multiple sources with clear precedence
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()              // Local dev only
    .AddEnvironmentVariables();             // Overrides everything
```

---

## 2. CI/CD Pipeline Secrets

### GitHub Actions Secrets

Set secrets at the repository or organization level:

```bash
# Using GitHub CLI
gh secret set AZURE_CLIENT_ID --body "your-client-id"
gh secret set AZURE_TENANT_ID --body "your-tenant-id"
gh secret set AZURE_CLIENT_SECRET --body "your-client-secret"

# Org-level secrets (shared across repos)
gh secret set --org obss SONAR_TOKEN --body "your-sonar-token"
```

### Usage in Workflows

```yaml
# security-scanning.yml
env:
  SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
  AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
  AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
```

**Never** use `echo` or logging to output secret values. Use `add-mask` if a secret might appear in logs:

```yaml
- name: Mask secrets in logs
  run: |
    echo "::add-mask::${{ secrets.AZURE_CLIENT_SECRET }}"
```

---

## 3. Production (Azure Key Vault / AWS Secrets Manager)

### Azure Key Vault

```csharp
// Program.cs - Azure Key Vault integration
builder.Configuration.AddAzureKeyVault(
    new Uri("https://obss-prod-kv.vault.azure.net/"),
    new DefaultAzureCredential());

// appsettings.json references Key Vault references
{
  "ConnectionStrings": {
    "DefaultConnection": "@Microsoft.KeyVault(SecretUri=https://obss-prod-kv.vault.azure.net/secrets/DefaultConnection/)"
  }
}
```

### AWS Secrets Manager

```csharp
// Program.cs - AWS Secrets Manager
builder.Configuration.AddSecretsManager(configurator: options =>
{
    options.SecretFilter = entry => entry.Name.StartsWith("obss/");
    options.KeyGenerator = (_, entry) => entry.Name
        .Replace("obss/", "")
        .Replace("/", ":");
});
```

### Access Control

```terraform
# Terraform: Key Vault access policy
resource "azurerm_key_vault_access_policy" "api_service" {
  key_vault_id = azurerm_key_vault.obss.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.api_service.principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}
```

- Each service gets only `Get` and `List` permissions on secrets it needs
- No service has `Set` or `Delete` permissions (operations team only)
- Managed identities are used instead of service principal credentials

---

## 4. Connection Strings

### SQL Server / Postgres

```bash
# Local (User Secrets)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost;Database=obss;User Id=sa;Password=local_dev_123;TrustServerCertificate=true"

# Production (Key Vault)
# Key Vault secret name: DefaultConnection
# Value: "Server=db.prod.example.com;Database=obss;User Id=svc_obss;Password=<actual>;"
```

### Redis

```bash
# Local (User Secrets)
dotnet user-secrets set "ConnectionStrings:Redis" \
  "localhost:6379,password=local_dev_redis"

# Production (Key Vault)
# Key Vault secret name: RedisConnection
```

### RabbitMQ

```bash
# Local (User Secrets)
dotnet user-secrets set "ConnectionStrings:RabbitMQ" \
  "amqp://user:password@localhost:5672/"
```

### OpenSearch

```bash
# Production (Key Vault)
# Key Vault secret name: OpenSearchConnection
# Value includes username + password
```

---

## 5. Key Rotation Policy

### Rotation Schedule

| Secret Type | Rotation Frequency | Method |
|-------------|-------------------|--------|
| Database passwords | Every 90 days | Manual rotation via DevOps |
| JWT signing keys | Every 180 days | Key Vault key rotation |
| API keys (third-party) | Every 90 days | Per vendor policy |
| TLS certificates | Every 365 days | Automated via cert-manager |
| CI/CD tokens | Every 90 days | GitHub token regeneration |
| Service principal secrets | Every 180 days | Azure AD app registration |

### Rotation Procedure

```bash
# 1. Generate new secret in Key Vault (versioned)
az keyvault secret set \
  --vault-name obss-prod-kv \
  --name DefaultConnection \
  --value "<new-connection-string>"

# 2. Deploy services with zero-downtime
#    Services pick up the new secret on restart/reload

# 3. Verify all services are using the new secret
#    (Check logs for connection success)

# 4. Rotate the actual database password
az postgres flexible-server update \
  --admin-password "<new-password>"

# 5. Remove old key version (after verification period)
az keyvault secret set-attributes \
  --vault-name obss-prod-kv \
  --name DefaultConnection \
  --version <old-version-id> \
  --enabled false
```

### Emergency Rotation

```bash
# Immediate rotation on suspected compromise
# 1. Disable compromised secret immediately
az keyvault secret set-attributes \
  --vault-name obss-prod-kv \
  --name DefaultConnection \
  --enabled false

# 2. Generate replacement
az keyvault secret set \
  --vault-name obss-prod-kv \
  --name DefaultConnection \
  --value "<new-value>"

# 3. Rotate actual credential at source
# 4. Audit logs for unauthorized access since last valid rotation
# 5. Notify security team
```

---

## 6. No Secrets in Code Policy

### Prohibited Patterns

```csharp
// ❌ NEVER DO THIS - Hardcoded secrets
var connectionString = "Server=prod;Database=obss;User Id=sa;Password=SuperSecret123!";

// ❌ NEVER DO THIS - Secrets in committed config
{
  "EmailSettings": {
    "ApiKey": "sk-live-abc123..."  // Actual key in committed file
  }
}

// ❌ NEVER DO THIS - Secrets in comments
// Password: P@ssw0rd! (in a code comment)
```

### Allowed Patterns

```csharp
// ✅ Environment variable
var apiKey = Environment.GetEnvironmentVariable("PAYMENT_API_KEY");

// ✅ Configuration (from User Secrets or Key Vault)
var connString = configuration.GetConnectionString("DefaultConnection");

// ✅ Options pattern with validated source
public class PaymentOptions
{
    public string ApiKey { get; set; }  // From Key Vault binding
}
```

### Automated Checks

```yaml
# CI pipeline secret detection (gitleaks / trufflehog)
- name: Secret Scan
  uses: gitleaks/gitleaks-action@v2
```

---

## 7. Secret Auditing

### What to Audit

- **Key Vault access logs** — monitor for unauthorized `Get` operations
- **Secret rotation** — verify all secrets are rotated within policy window
- **User Secret usage** — ensure developers use User Secrets, not config files

### Audit Commands

```bash
# Azure Key Vault audit (via Diagnostic Settings -> Log Analytics)
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.KEYVAULT"
| where OperationName == "SecretGet"
| where ResultType != "Success"
| project TimeGenerated, Identity, CallerIPAddress, ResultSignature

# Check for secrets in codebase
grep -rn "password\s*=" --include="*.cs" src/ --exclude-dir=obj | grep -v "\.example"
grep -rn "ConnectionString" --include="*.cs" src/ | grep -v "GetConnectionString" | grep -v "\.example"
grep -rn "AKIA[0-9A-Z]{16}" --include="*" .   # AWS access keys
grep -rn "ghp_[a-zA-Z0-9]{36}" --include="*" .  # GitHub tokens
```

---

## 8. Incident Response for Secrets

| Situation | Response |
|-----------|----------|
| Secret committed to git | Rotate immediately. Use `bfg-repo-cleaner` to remove from history. Notify all developers to rebase. |
| Secret exposed in logs | Rotate immediately. Patch logging configuration. |
| Key Vault breach | Rotate ALL secrets. Audit access logs. Notify compliance team. |
| Developer laptop compromised | Rotate all secrets the dev had access to. Revoke their credentials. |

### Git Secret Removal

```bash
# Remove a file containing secrets from git history
bfg --delete-files appsettings.Production.json

# Replace text in history
bfg --replace-text secrets.txt

# Force push after cleanup
git push --force --all
```

---

## Appendix: Quick Reference

| Task | Command |
|------|---------|
| Set user secret | `dotnet user-secrets set "Key" "Value"` |
| List user secrets | `dotnet user-secrets list` |
| Clear user secrets | `dotnet user-secrets clear` |
| Set GitHub secret | `gh secret set NAME --body "value"` |
| List GitHub secrets | `gh secret list` |
| Get Key Vault secret | `az keyvault secret show --vault-name obss-kv --name SecretName` |
| Set Key Vault secret | `az keyvault secret set --vault-name obss-kv --name SecretName --value "value"` |
| Scan for secrets | `gitleaks detect --source .` |
