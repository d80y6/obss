# EF Core Migrations

## Prerequisites

Install the EF Core CLI tool:

```bash
dotnet tool install --global dotnet-ef
```

Verify installation:

```bash
dotnet ef --version
```

## Adding a Migration

```bash
# IAM Module
dotnet ef migrations add <MigrationName> \
  -p src/Modules/IAM/Obss.IAM.Infrastructure \
  -s src/Host/Obss.Host \
  -c IamDbContext

# Example:
dotnet ef migrations add InitialCreate \
  -p src/Modules/IAM/Obss.IAM.Infrastructure \
  -s src/Host/Obss.Host \
  -c IamDbContext
```

## Applying Migrations

```bash
# Apply all pending migrations
dotnet ef database update \
  -p src/Modules/IAM/Obss.IAM.Infrastructure \
  -s src/Host/Obss.Host \
  -c IamDbContext

# Apply to a specific migration
dotnet ef database update <MigrationName> \
  -p src/Modules/IAM/Obss.IAM.Infrastructure \
  -s src/Host/Obss.Host \
  -c IamDbContext
```

## Removing a Migration

```bash
dotnet ef migrations remove \
  -p src/Modules/IAM/Obss.IAM.Infrastructure \
  -s src/Host/Obss.Host \
  -c IamDbContext
```

## Generating Scripts

```bash
# Generate SQL script from all migrations
dotnet ef migrations script \
  -p src/Modules/IAM/Obss.IAM.Infrastructure \
  -s src/Host/Obss.Host \
  -c IamDbContext \
  -o infrastructure/database/migrations/iam-migrations.sql

# Generate script between two migrations
dotnet ef migrations script <FromMigration> <ToMigration> \
  -p src/Modules/IAM/Obss.IAM.Infrastructure \
  -s src/Host/Obss.Host \
  -c IamDbContext \
  -o infrastructure/database/migrations/iam-migrations.sql
```

## General Migration Workflow

1. Modify domain entities in the relevant module
2. Add or update entity type configurations
3. Add a new migration with a descriptive name
4. Review the generated migration code
5. Test by applying to a development database
6. Commit the migration code to source control

## Module-Specific Commands

| Module          | DbContext       | Project Path                                               |
|----------------|-----------------|------------------------------------------------------------|
| IAM            | IamDbContext    | src/Modules/IAM/Obss.IAM.Infrastructure                    |
| CRM            | CrmDbContext    | src/Modules/CRM/Obss.CRM.Infrastructure                    |
| ProductCatalog | CatalogDbContext| src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure |
| Billing        | BillingDbContext| src/Modules/Billing/Obss.Billing.Infrastructure            |
| Collections    | CollectionsDbContext | src/Modules/Collections/Obss.Collections.Infrastructure |

Example for CRM module:

```bash
dotnet ef migrations add InitialCreate \
  -p src/Modules/CRM/Obss.CRM.Infrastructure \
  -s src/Host/Obss.Host \
  -c CrmDbContext
```
