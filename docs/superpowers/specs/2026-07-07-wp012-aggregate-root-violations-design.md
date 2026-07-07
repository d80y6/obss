# WP-012: Fix Aggregate Root Violations

**Date:** 2026-07-07
**Status:** Draft → Approved
**Roadmap Ref:** Wave 1, WP-012
**Goal:** Fix DDD aggregate root violations across 4 modules: ProductCatalog, EventManagement, Subscriptions, Audit, NetworkInventory, Workflow, and Invoices.

---

## 1. Promotions (Entity → AggregateRoot)

Three entities currently extend `Entity<Guid>` but have their own repositories (a DDD violation). Since `AggregateRoot<TId>` extends `Entity<TId>` and only adds `IntegrationEvent` support, the promotion is purely a base-class change with zero behavioral impact.

| Entity | File | Current Base | New Base |
|--------|------|-------------|----------|
| `Category` | `ProductCatalog.Domain/Domain/Entities/Category.cs` | `Entity<Guid>` | `AggregateRoot<Guid>` |
| `WebhookEvent` | `EventManagement.Domain/Entities/WebhookEvent.cs` | `Entity<Guid>` | `AggregateRoot<Guid>` |
| `SubscriptionEntitlement` | `Subscriptions.Domain/Entities/SubscriptionEntitlement.cs` | `Entity<Guid>` | `AggregateRoot<Guid>` |

**Changes per entity:**
- Change `: Entity<Guid>` to `: AggregateRoot<Guid>` in class declaration
- The `: base(id)` constructor calls work unchanged (AggregateRoot has the same constructor signature)
- No repository changes needed (repositories are valid for aggregate roots)

**SubscriptionEntitlement** already raises domain events (`EntitlementUpdatedDomainEvent`, `EntitlementLimitReachedDomainEvent`) via `AddDomainEvent()` — this is correct behavior for an AggregateRoot and requires no changes.

---

## 2. Demotions (AggregateRoot → Entity)

Four entities currently extend `AggregateRoot<Guid>` but should be modeled as child entities. They lack domain events or integration events that would justify aggregate root status.

| Entity | File | Current Base | New Base | Repo Action |
|--------|------|-------------|----------|------------|
| `AuditPolicy` | `Audit.Domain/Entities/AuditPolicy.cs` | `AggregateRoot<Guid>` | `Entity<Guid>` | Remove `IRepository<AuditPolicy>` refs from handlers |
| `AuditAlertRule` | `Audit.Domain/Entities/AuditAlertRule.cs` | `AggregateRoot<Guid>` | `Entity<Guid>` | Delete `IAuditAlertRuleRepository` + impl |
| `TopologyMap` | `NetworkInventory.Domain/Entities/TopologyMap.cs` | `AggregateRoot<Guid>` | `Entity<Guid>` | Delete `TopologyMapRepository` |
| `WorkflowMetric` | `Workflow.Domain/Entities/WorkflowMetric.cs` | Already `Entity<Guid>` ✅ | No base change | Delete `IWorkflowMetricRepository` + impl |

**Changes per entity:**

### AuditPolicy (`Audit.Domain`)
- Change base from `AggregateRoot<Guid>` to `Entity<Guid>`
- AuditPolicy has no domain events or integration events — clean demotion
- No dedicated repository interface exists (handlers use `IRepository<AuditPolicy>`), so no files to delete
- Find all command/query handlers injecting `IRepository<AuditPolicy>` and change them to use `IAuditLogRepository` or direct EF, since a non-aggregate Entity should not have its own repository

### AuditAlertRule (`Audit.Domain`)
- Change base from `AggregateRoot<Guid>` to `Entity<Guid>`
- No domain events or integration events
- Delete `IAuditAlertRuleRepository` interface and `AuditAlertRuleRepository` implementation
- Update all handlers injecting `IAuditAlertRuleRepository`

### TopologyMap (`NetworkInventory.Domain`)
- Change base from `AggregateRoot<Guid>` to `Entity<Guid>`
- No domain events or integration events
- Delete `TopologyMapRepository` (no interface exists)
- Update all handlers using `TopologyMapRepository`

### WorkflowMetric (`Workflow.Domain`)
- Already `Entity<Guid>` — no base class change needed
- Delete `IWorkflowMetricRepository` interface and `WorkflowMetricRepository` implementation
- Update all handlers injecting `IWorkflowMetricRepository`

---

## 3. Domain Event Location Fix

**InvoiceCancelledDomainEvent** is defined inline at the bottom of `Invoice.cs` (line 278). It must be extracted to its own file.

| Current Location | Target Location |
|----------------|-----------------|
| `Invoices.Domain/Domain/Entities/Invoice.cs` (inline) | `Invoices.Domain/Domain/Events/InvoiceCancelledDomainEvent.cs` |

The `Events/` folder already exists with 6 domain event files (`CreditNoteIssuedDomainEvent.cs`, `InvoiceDisputeOpenedDomainEvent.cs`, `InvoiceDisputeResolvedDomainEvent.cs`, `InvoiceFinalizedDomainEvent.cs`, `InvoiceOverdueDomainEvent.cs`, `InvoicePaidDomainEvent.cs`) — follow the same pattern.

**Note:** `InvoicePayment` and `InvoiceNote` are also defined inline in `Invoice.cs` but are child entities of Invoice, not domain events. They are out of scope for this WP.

---

## 4. Out of Scope

- `IRepository<T>` constraint: Remains `where T : class`. Tightening to `where T : AggregateRoot` would break handlers for demoted entities and is a separate concern.
- `InvoicePayment` and `InvoiceNote` inline definitions: Entity child types, not errors to fix here.
- `EfRepository<T>` implementation: No changes to the generic repository pattern.
