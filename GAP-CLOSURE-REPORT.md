# Gap Closure Report

## 1. Build Failures — RESOLVED

| Issue | File | Fix |
|-------|------|-----|
| Missing exception constructors | `OCS/Domain/Exceptions/OcsDomainExceptions.cs` | Added parameterless ctor, `(string, Exception)` ctor, serialization ctor, `[Serializable]` to `InsufficientBalanceException` and `CreditPoolExhaustedException` |

**Build**: `Build succeeded. 0 Warning(s) 0 Error(s)` — all 25 modules + host + shared kernel

## 2. Database Migrations — 3 NEW

| Module | Migration | Entities |
|--------|-----------|---------|
| **AAA** | `InitialCreate` | RadiusSession, NetworkAccessServer (RADIUS accounting) |
| **EventManagement** | `InitialCreate` | EventSubscription, WebhookEvent |
| **OCS** | `InitialCreate` | Balance, CreditPool, OcsTransaction |

Design-time `DbContextFactory` classes created for all 3 modules. `Microsoft.EntityFrameworkCore.Design` added (PrivateAssets=all).

## 3. Provisioning Transport Layer — NEW (SNMP/SSH/NETCONF/REST)

| Component | File | Details |
|-----------|------|---------|
| `TransportProtocol` enum | `Transports/Abstractions/TransportProtocol.cs` | Snmp, Ssh, Netconf, Rest |
| `INetworkTransport` | `Transports/Abstractions/INetworkTransport.cs` | Generic transport interface |
| `ITransportFactory` | `Transports/Abstractions/ITransportFactory.cs` | Resolves by protocol |
| `TransportResult` / `TransportConnectionResult` | `Transports/Abstractions/` | Result types |
| `SnmpTransport` | `Transports/Snmp/SnmpTransport.cs` | v1/v2c/v3 via Lextm.SharpSnmpLib |
| `SshTransport` | `Transports/Ssh/SshTransport.cs` | Password + key auth via SSH.NET |
| `NetconfTransport` | `Transports/Netconf/NetconfTransport.cs` | RFC 6241 over SSH, XML RPC |
| `RestTransport` | `Transports/Rest/RestTransport.cs` | GET/POST/PUT/PATCH/DELETE, Basic/Bearer/APIKey |
| `TransportFactory` | `Transports/TransportFactory.cs` | DI-resolved factory |
| `TransportHealthCheck` | `Transports/TransportHealthCheck.cs` | ASP.NET health check |
| DI extensions | `Transports/Extensions/TransportServiceCollectionExtensions.cs` | `AddNetworkTransports()` |
| NuGet packages | `Directory.Packages.props` | SharpSnmpLib 12.5.7, SSH.NET 2024.2.0, Microsoft.Extensions.Http 9.4 |

## 4. Huawei Vendor Adapter — REAL (not stub)

| Component | File | Details |
|-----------|------|---------|
| `HuaweiBroadbandAdapter` | `Adapters/Huawei/HuaweiBroadbandAdapter.cs` | Real adapter using transport layer. Auto-selects NETCONF, SSH/CLI, SNMP, or REST based on config. Returns `AdapterResult.Blocked` if no transport configured + simulator off |
| `HuaweiProvisioningAdapter` | `Services/HuaweiProvisioningAdapter.cs` | Bridges provisioning pipeline to `IHuaweiBroadbandAdapter` |
| `HuaweiAdapterConfig` | Updated | Added `UseSimulator` flag, transport configs, `TryGet*()` methods |

## 5. Multi-Tenancy — 13 ENTITIES FIXED

`ITenantEntity` added to entities that already had `string TenantId` (compile-time only, no migration needed):

| Module | Entities |
|--------|----------|
| **OCS** | Balance, CreditPool, OcsTransaction |
| **Billing** | Bill, BillingCycle |
| **Payments** | Payment |
| **CRM** | Customer |
| **Ticketing** | Ticket |
| **Orders** | ProductOrder |
| **Subscriptions** | Subscription |
| **Collections** | CollectionCase, DunningPolicy |
| **Rating** | UsageRecord, Promotion, RatingRule |

These now benefit from automatic `EfDbContext.ApplyTenantQueryFilters()` global query filters and `SetTenantId()` on save.

## 6. Frontend Pages — 10 NEW

| Module | Pages |
|--------|-------|
| **OCS** | Landing, Balances list, Balance detail (with adjust form), Credit pools list, Credit pool detail, Transactions list |
| **EventManagement** | Landing, Subscriptions list (with create form), Subscription detail, Webhook events list |

API hooks (`useOcs.ts`, `useEventManagement.ts`) and query keys added. Sidebar updated with OCS (Wallet icon) and Events (Radio icon) entries.

## 7. Multi-Tenancy — Guid Entities via ITenantEntity (COMPLETED)

`ITenantEntity` added via explicit interface implementation to Guid-based entities:

| Module | Entities | Approach |
|--------|----------|----------|
| **Provisioning** | ServiceOrder, ProvisioningJob, ProvisioningTemplate | `string ITenantEntity.TenantId => TenantId.ToString("N")` |
| **ServiceInventory** | Service, ResourceDiscoveryJob | Same |
| **Subscriptions** | Product | Same |

`EfDbContext.ApplyTenantQueryFilters` updated to use `Expression.Convert` for interface cast, enabling filter across both `string` and `Guid` TenantId entity types. `SetTenantId` updated to skip Guid entities.

## 8. Missing TenantId Properties (COMPLETED)

| Entity | Module | Change |
|--------|--------|--------|
| **BillingAccount** | Billing | Added `string TenantId` property, constructor param, config |
| **AccountBalance** | Billing | Added `string TenantId` property, constructor param, config |
| **CdrRecord** | Rating | Added `string TenantId` property, constructor param, config, factory |

Migrations generated for Billing (`AddTenantId`) and Rating (`AddTenantId`).

## 9. Invoices TenantId Value Object (COMPLETED)

| Change | Files |
|--------|-------|
| Type change: `TenantId` (value object) → `string` | Invoice.cs, CreditNote.cs |
| Removed `.HasConversion<TenantIdValueConverter>()` | InvoiceConfiguration.cs, CreditNoteConfiguration.cs |
| Updated callers | 3 command handlers, 1 mapping config, 1 integration event handler |
| Updated tests | 10 test files updated (28 errors fixed) |
| Migration generated | Includes outbox message column additions |

## 10. Integration Tests
- 11 integration tests fail (6 Workflow, 5 IAM) — require Docker PostgreSQL, unrelated to changes
