# WP-018: ServiceOrder (TMF641) for Provisioning Module

**Date:** 2026-07-08
**Status:** Approved

## Goal

Add a TMF641-compliant `ServiceOrder` aggregate root to the Provisioning module, wrapping the existing Job/Task execution engine. Enable lifecycle state management, cancellation, item tracking, and bidirectional integration with the Orders module.

## Architecture

**Approach:** Layered — ServiceOrder and ServiceOrderItem are new domain entities. Existing ProvisioningJob and ProvisioningTask remain as the execution engine underneath. ServiceOrderItem triggers one or more ProvisioningJobs. Outbound integration events notify Orders of state changes.

```
Orders ──ProvisioningRequired→ Provisioning (ServiceOrder AR)
                                     │
                              ServiceOrderItem
                                     │ (triggers)
                              ProvisioningJob (existing)
                                     │
                              ProvisioningTask (existing)
                                     │
                              ServiceInventory activation
                                     │
Orders ←─ServiceOrderStateChanged───┘
```

## Domain

### ServiceOrder (AggregateRoot)

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | |
| TenantId | TenantId | |
| ExternalId | string? | Reference to Orders order ID |
| State | ServiceOrderState | TMF641 lifecycle (see below) |
| Priority | string? | |
| Description | string? | |
| Category | string? | |
| RelatedParty | List\<RelatedPartyRef\> | Party name, role, ID |
| Characteristics | List\<CharValue\> | Key-value characteristics |
| Notes | List\<Note\> | Timestamped notes |
| Milestones | List\<Milestone\> | Name, date, status |
| RequestedStartDate | DateTime? | |
| RequestedCompletionDate | DateTime? | |
| OrderDate | DateTime | |
| StatusChangeDate | DateTime? | |
| ExternalId | string? | TMF external ID |
| Items | List\<ServiceOrderItem\> | |
| CancelRequest | CancelServiceOrder? | |

**States (TMF641):** `Acknowledged` → `InProgress` ⇄ `Held` → `Completed` | `Cancelled` | `Failed` | `PartiallyCompleted`. Additional: `Rejected`, `PendingCancellation`.

### ServiceOrderItem (Entity, owned by ServiceOrder)

| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | |
| ServiceId | Guid? | Reference to ServiceInventory service |
| Action | ServiceOrderAction | Add, Modify, Delete |
| State | ServiceOrderItemState | Per-item lifecycle |
| Quantity | int | |
| Appointment | Appointment? | Time slot |
| RelatedParty | List\<RelatedPartyRef\> | |
| RequestedStartDate | DateTime? | |
| RequestedCompletionDate | DateTime? | |
| CompletedDate | DateTime? | |

**Actions:** `Add`, `Modify`, `Delete`
**States:** `Acknowledged`, `InProgress`, `Completed`, `Failed`, `Held`, `Cancelled`

### CancelServiceOrder (Entity, owned by ServiceOrder)

| Property | Type |
|----------|------|
| Id | Guid |
| Reason | string |
| RequestedCompletionDate | DateTime? |
| CompletedDate | DateTime? |
| State | string |

### Milestone (ValueObject)

| Property | Type |
|----------|------|
| Name | string |
| Description | string? |
| Date | DateTime |
| Status | MilestoneStatus (Pending, Reached, Missed, Cancelled) |

### Domain Events

- `ServiceOrderSubmittedDomainEvent` — on initial creation
- `ServiceOrderStateChangedDomainEvent` — on any state transition
- `ServiceOrderItemCompletedDomainEvent` — per-item when job completes
- `ServiceOrderCancellationRequestedDomainEvent`

### State Machine

```
                     ┌──→ Cancelled
Acknowledged ──→ InProgress ──→ Completed
     │              │  │          ├→ PartiallyCompleted
     │              │  │          └→ Failed
     │              │  │
     └──→ Rejected  │  └──→ Held ──→ InProgress
                    │
                    └──→ PendingCancellation ──→ Cancelled
```

Transitions guarded by domain invariants:
- `InProgress`: requires at least one item
- `Completed`: all items completed
- `Held`: can resume to InProgress
- `Cancelled`: no items in progress
- `Failed`: one or more items failed

## Application

### Commands

- `CreateServiceOrderCommand` — creates ServiceOrder with items, triggers initial ProvisioningJobs
- `UpdateServiceOrderCommand` — PATCH fields
- `CancelServiceOrderCommand` — requests cancellation
- `CompleteServiceOrderItemCommand` — marks item done (called by job completion handler)

### Queries

- `GetServiceOrderByIdQuery`
- `GetServiceOrdersQuery` — list with filters (state, dates, pagination)
- `GetServiceOrderItemsQuery` — items for an order

### Handlers

- `CreateServiceOrderCommandHandler` — creates ServiceOrder, for each item creates ProvisioningJob using existing template matching
- `CompleteServiceOrderItemCommandHandler` — called when ProvisioningJob completes, updates item state, updates ServiceOrder state
- `CancelServiceOrderCommandHandler` — sets PendingCancellation, aborts active ProvisioningJobs

## Integration Events

### Inbound (from Orders)

- `ProvisioningRequiredIntegrationEvent` — **extended**: now carries item details. Handler creates ServiceOrder instead of a single ProvisioningJob.

### Outbound (to Orders, NEW)

- `ServiceOrderSubmittedIntegrationEvent` — when acknowledged
- `ServiceOrderStateChangedIntegrationEvent` — state transitions (with order ID, new state)
- `ServiceOrderItemCompletedIntegrationEvent` — per-item completion (order ID, item ID, service ID)

## API Endpoints

All under `/api/v{version}/provisioning/service-orders`:

| Method | Path | Description |
|--------|------|-------------|
| POST | `/service-orders` | Create |
| GET | `/service-orders` | List (paginated, filterable) |
| GET | `/service-orders/{id}` | Get by ID |
| PATCH | `/service-orders/{id}` | Update fields |
| DELETE | `/service-orders/{id}` | Delete (only if Acknowledged) |
| POST | `/service-orders/{id}/cancel` | Request cancellation |
| GET | `/service-orders/{id}/items` | List items |
| GET | `/service-orders/{id}/items/{itemId}` | Get item by ID |

Existing Job/Task/Template endpoints remain unchanged.

## Database

New tables in `provisioning` schema:

- `service_orders` — ServiceOrder entity
- `service_order_items` — ServiceOrderItem entity (FK → service_orders)
- `service_order_related_parties` — JSON-like related party
- `service_order_characteristics` — characteristic key-value
- `service_order_milestones` — milestone tracking
- `service_order_cancellations` — Cancellation requests
- `service_order_notes` — Notes

**Migration:** Additive only. No changes to existing `provisioning_jobs` or `provisioning_tasks` tables.

**FK from `service_order_items` to `provisioning_jobs`:** Optional, through a join table or as a reference on the job. Simpler: add a nullable `service_order_item_id` column to `provisioning_jobs`.

## Testing

- Unit tests for state machine transitions
- Unit tests for command handlers
- Integration test: create ServiceOrder → verify Jobs created → complete Jobs → verify item/order state changes
- Integration test: cancellation flow
- Verify no existing tests break (additive change)

## Out of Scope

- Frontend pages (existing Job pages remain sufficient for now)
- Real provisioning engine integration (stays MOCK)
- TMF648 Quote integration (separate WP)
