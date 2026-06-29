# EF Core Migrations Conventions

## Adding a Migration

```bash
dotnet ef migrations add MigrationName \
  --project src/Modules/{ModuleName}/Obss.{ModuleName}.Infrastructure \
  --startup-project src/Gateway/Obss.Gateway \
  --output-dir Infrastructure/Persistence/Migrations
```

## Naming Conventions

- Use descriptive PascalCase names: `CreateUserTable`, `AddBillingStatusColumn`
- Prefix with module area when ambiguous: `IAM_CreateRoleTable`
- Use past tense: `AddedIndexOnEmail`, `RenamedSubscriptionStatus`

## Migration Structure

- Each module owns its own `EfDbContext` subclass and migration set
- All modules target the same PostgreSQL database
- Use the module's `MigrationsAssembly` when registering in `AddEntityFramework`
- Shared entities (OutboxMessage, etc.) are configured in the shared `EfDbContext`

## Code-First Rules

- All entity properties use `SnakeCase` naming convention (handled by `UseSnakeCaseNamingConvention` in `OnConfiguring`)
- Tenant-scoped entities implement `ITenantEntity`
- Configure indexes for all foreign key columns used in queries
- Use `HasColumnType("jsonb")` for JSON columns
- Use `HasMaxLength` for all string properties
- Do not use `ValueGeneratedNever()` for non-key properties
- All primary keys are `Guid` by default

## Review Checklist

- [ ] Migration generates valid PostgreSQL (not SQL Server) SQL
- [ ] No hardcoded schema names
- [ ] Indexes exist on frequently queried columns
- [ ] Down migration is safe (no data loss without warning)
- [ ] Foreign keys have proper delete behavior (Restrict by default)
