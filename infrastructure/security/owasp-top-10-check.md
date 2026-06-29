# OWASP Top 10 (2021) Verification Checklist

> **Platform:** Telecom OSS/BSS Platform
> **Last Review:** 2026-06-21
> **Owner:** Security Team

---

## A01: Broken Access Control

### RBAC Verification

- [ ] All API endpoints enforce `[Authorize(Roles = "...")]` or policy-based authorization
- [ ] No endpoint relies solely on frontend hiding UI elements for access control
- [ ] Role-to-permission mapping is centralized (not scattered across controllers)
- [ ] Default behavior is **deny** — endpoints without explicit allow attributes return 403
- [ ] Role elevation / privilege escalation is tested (user cannot self-assign admin roles)
- [ ] Horizontal escalation tests: User A cannot access User B's resources

### Tenant Isolation Test Cases

```gherkin
Scenario: Cross-tenant data access
  Given tenant "Alpha" is authenticated
  When GET /api/tenants/{betaId}/subscribers is requested
  Then response status is 403 (or 404)

Scenario: Tenant admin scoping
  Given a tenant-admin user for "Alpha"
  When they access tenant-admin endpoints
  Then only their own tenant's data is returned
```

- [ ] Tenant ID is derived from the authenticated principal, never from request body/query
- [ ] Multi-tenant database queries always include `WHERE TenantId = @tenantId`
- [ ] Admin API for global operations is separated from tenant-scoped APIs

### CORS & HTTP Methods

- [ ] `OPTIONS` preflight properly restricted
- [ ] `PUT`, `DELETE`, `PATCH` are not exposed on read-only endpoints
- [ ] CORS `AllowOrigins` is explicit (not `*`) in production

---

## A02: Cryptographic Failures

### TLS Configuration

- [ ] TLS 1.2 minimum enforced; TLS 1.0/1.1 disabled
- [ ] Valid certificate from trusted CA in all environments (not self-signed in prod)
- [ ] HSTS header present with `max-age=31536000; includeSubDomains`
- [ ] Internal service-to-service communication uses mTLS

### Secrets Management

```bash
# Verify no secrets in code
grep -r "password\s*=" --include="*.cs" --include="*.json" src/ --exclude-dir=obj
grep -r "connectionString" --include="*.cs" src/ --exclude-dir=obj | grep -v "UserSecret"
grep -rn "AKIA" --include="*" .  # AWS keys
```

- [ ] Connection strings use secrets or environment variables, never appsettings.json committed
- [ ] API keys, tokens, and passwords stored in Azure Key Vault / AWS Secrets Manager
- [ ] No hardcoded credentials in any committed file (including tests)

### Password Hashing

- [ ] Passwords hashed with **bcrypt**, **Argon2id**, or **PBKDF2** (not MD5, SHA-1, unsalted SHA-256)
- [ ] Salt is unique per password
- [ ] Work factor / iteration count is sufficient (bcrypt cost >= 10)
- [ ] Password length limit is reasonable (128+) but NOT truncated before hashing

---

## A03: Injection

### SQL Injection (EF Core)

```csharp
// SAFE - parameterized
var user = await context.Users
    .FirstOrDefaultAsync(u => u.Email == email);

// DANGEROUS - Raw SQL interpolation
var user = await context.Users
    .FromSqlRaw($"SELECT * FROM Users WHERE Email = '{email}'")  // VULNERABLE
    .FirstOrDefaultAsync();
```

- [ ] All database access goes through EF Core LINQ or parameterized `FromSqlInterpolated`
- [ ] No `FromSqlRaw` or `ExecuteSqlRaw` with string concatenation
- [ ] Stored procedures use parameterized calls
- [ ] Dynamic SQL generation is NOT used anywhere in the codebase

### XSS Prevention

- [ ] All user-supplied data is HTML-encoded before rendering
- [ ] `Content-Type: text/html` is NOT used for API responses containing user data
- [ ] CSP headers are present (see A05)
- [ ] `@` Razor syntax is used instead of `Html.Raw()` for user content
- [ ] API responses return JSON, not HTML fragments

### Command Injection

- [ ] `Process.Start()` is never called with user-supplied input
- [ ] If shell commands are required, arguments are validated against a strict allowlist
- [ ] File paths from user input are sanitized (no `../`, no null bytes)

---

## A04: Insecure Design

### Rate Limiting

- [ ] Rate limiting is applied to all public API endpoints
- [ ] Authentication endpoints have stricter limits (e.g., 5 req/min per IP)
- [ ] Rate limit headers returned: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`
- [ ] Rate limiting is enforced server-side (not just client/frontend)

```csharp
// Example: ASP.NET Core rate limiting middleware
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});
```

### Input Validation

- [ ] All inputs validated against a schema (FluentValidation / DataAnnotations)
- [ ] File upload size limits enforced (max 10 MB for API uploads)
- [ ] Allowed file type extensions are explicitly listed (deny-list is insufficient)
- [ ] GUIDs and IDs are validated (format check) before DB query
- [ ] Pagination limits enforced (max page size, offset caps)

---

## A05: Security Misconfiguration

### CORS Configuration

```json
{
  "AllowedOrigins": ["https://portal.obss.example.com"]
}
```

- [ ] CORS is not `AllowAnyOrigin()` in production
- [ ] Exposed headers are minimal (no internal headers leaked)
- [ ] `AllowCredentials()` is only set when specific origins are listed

### Security Headers

| Header | Expected Value |
|--------|---------------|
| `Strict-Transport-Security` | `max-age=31536000; includeSubDomains` |
| `X-Content-Type-Options` | `nosniff` |
| `X-Frame-Options` | `DENY` |
| `Content-Security-Policy` | `default-src 'self'` |
| `X-XSS-Protection` | `0` (deprecated, rely on CSP) |
| `Referrer-Policy` | `strict-origin-when-cross-origin` |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=()` |

- [ ] All headers present in middleware pipeline (not manual per-controller)

### Error Handling

- [ ] Custom error pages / responses do not expose stack traces or internal paths
- [ ] 500 responses return generic messages in production
- [ ] Exception logging includes full details, but HTTP response does not
- [ ] Developer exception page (`UseDeveloperExceptionPage`) is disabled in production

### Default Credentials

- [ ] All default passwords changed on deployed services (Postgres, Redis, RabbitMQ, Keycloak)
- [ ] Vendor default accounts are disabled or removed
- [ ] Docker images do not include default passwords

---

## A06: Vulnerable & Outdated Components

### Package Scanning

```bash
# NuGet vulnerability audit
dotnet list package --vulnerable --include-transitive

# Check for outdated packages
dotnet list package --outdated
```

- [ ] `dotnet list package --vulnerable` returns zero critical/high findings
- [ ] All dependencies are pinned to specific versions (no floating `*` or major-only)
- [ ] Transitive dependencies with known CVEs are overridden via `Directory.Packages.props`

### Dependency Audit Schedule

- [ ] NuGet packages audited at least weekly (automated via Dependabot / Renovate)
- [ ] Critical CVEs patched within 24 hours
- [ ] High CVEs patched within 7 days
- [ ] Medium/Low CVEs patched on next release cycle

### Container Images

- [ ] Base images are pinned by digest (`mcr.microsoft.com/dotnet/aspnet:9.0@sha256:...`)
- [ ] Images are scanned with Trivy before deployment
- [ ] Distroless or slim base images preferred over full OS images

---

## A07: Identification & Authentication Failures

### Token Validation

```csharp
// JWT validation must include these checks
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1),  // not 5
        };
    });
```

- [ ] JWT tokens are validated for issuer, audience, expiry, and signing key
- [ ] Clock skew is minimized (<= 1 minute)
- [ ] Tokens are short-lived (access tokens: 15 min, refresh tokens: 7 days)
- [ ] Refresh tokens are revokable (stored in DB with invalidation support)

### Session Management

- [ ] Session timeout enforced (15 min inactivity)
- [ ] Session IDs are cryptographically random
- [ ] Logout invalidates session on server side
- [ ] No session fixation (new session ID after login)

### Multi-Factor Authentication

- [ ] MFA is available and enforced for admin roles
- [ ] MFA recovery codes are one-time-use
- [ ] Rate limiting on MFA verification attempts

---

## A08: Software & Data Integrity Failures

### CI/CD Pipeline Security

- [ ] All CI/CD pipelines require signed commits or PR approval
- [ ] Secrets are injected at build time (not stored in repo)
- [ ] Pipeline artifacts are checksum-verified
- [ ] Deployment requires approval gate for production
- [ ] Third-party GitHub Actions pinned to commit SHA (not tags)

### Supply Chain

- [ ] NuGet packages come from trusted feeds only (no random public feeds)
- [ ] `nuget.config` restricts sources
- [ ] Package signature validation enabled where available
- [ ] SBOM (SPDX) generated per release

### Signed Commits

- [ ] All commits in `main` are signed (GPG or SSH)
- [ ] Branch protection enforces signed commits
- [ ] CI fails on unsigned commits

---

## A09: Logging & Monitoring

### Audit Trail

- [ ] All authentication events are logged (login, logout, failure, MFA)
- [ ] All authorization failures are logged (403 responses)
- [ ] All admin/sensitive operations are logged with user ID, timestamp, and action
- [ ] Logs include correlation ID for request tracing
- [ ] Logs never include passwords, tokens, PII, or secrets

### Logging Requirements

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    }
  }
}
```

- [ ] Structured logging (Serilog or similar) is used throughout
- [ ] Centralized log aggregation (OpenSearch / ELK) configured
- [ ] Retention policy defined (logs retained for minimum 90 days)
- [ ] Alerts configured for critical log patterns ("authentication failure rate spike")

### Monitoring

- [ ] Alert on: 5xx rate > 1%, 401/403 spikes, latency p99 > 2s
- [ ] Dashboard for security events (failed logins, rate limit hits)
- [ ] Dead letter queue monitoring for async message processing

---

## A10: Server-Side Request Forgery (SSRF)

### Request Validation

```csharp
// DENY LIST approach is insufficient — use ALLOW LIST
var allowedHosts = new HashSet<string>
{
    "api.thirdparty.example.com",
    "cdn.example.com"
};

// Validate before making outgoing requests
if (!allowedHosts.Contains(requestUri.Host))
    throw new InvalidOperationException("Host not allowed");
```

- [ ] Outbound HTTP requests go through a central HttpClient factory with URL validation
- [ ] Allowlist of permitted external hosts (deny-list alone is insufficient)
- [ ] Internal metadata endpoints (169.254.169.254, 10.x, 172.16-31.x, 192.168.x) are blocked
- [ ] URL redirects are NOT followed automatically
- [ ] DNS rebinding protection (validate host, not just IP)

### Cloud Metadata Protection

| Cloud | Metadata Endpoint | Block Method |
|-------|------------------|--------------|
| AWS | `http://169.254.169.254/latest/meta-data/` | Network ACL + app-level block |
| Azure | `http://169.254.169.254/metadata/` | Network ACL + app-level block |
| GCP | `http://metadata.google.internal/` | Network ACL + app-level block |

- [ ] Metadata endpoints are blocked by network policy (firewall / security group)
- [ ] Application-level block as defense-in-depth

---

## Scanning & Verification

### Automated Scanning Schedule

| Scan Type | Frequency | Tool |
|-----------|-----------|------|
| SAST (Static Analysis) | Every PR | Roslyn Analyzers / SonarCloud |
| Dependency Scan | Weekly | `dotnet list package --vulnerable` |
| Container Scan | Every build | Trivy |
| Secret Scan | Every PR | GitLeaks / TruffleHog |
| DAST | Monthly | OWASP ZAP |

### Manual Verification Cadence

- [ ] Penetration test: Quarterly (external)
- [ ] Code review for auth/authz logic: Every PR
- [ ] Infrastructure security review: Monthly
- [ ] Security training: Annually for all developers

---

## Evidence Log

| Date | Check | Result | Tester |
|------|-------|--------|--------|
| | | | |
