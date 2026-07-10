# WP-024: Product Order (TMF622) — Design Spec

**Date:** 2026-07-10
**Module:** `Obss.Orders` (enhancement)
**TMF API:** TMF622 Product Ordering
**Scope:** Full TMF622 compliance — rename, item state machine, relationships, milestones, BillingAccount ref, TMF attribute alignment.

---

## 1. Domain Model — Aggregate Rename & Enhance

### 1.1 Rename: `Order` → `ProductOrder`

The existing `Order` aggregate root is renamed to `ProductOrder` to match TMF622's primary resource. All entities, value objects, DTOs, commands, queries, integration events, EF configs, and repository interfaces are renamed accordingly.

| Current | New |
|---------|-----|
| `Order` (AggregateRoot) | `ProductOrder` |
| `OrderItem` (Entity) | `ProductOrderItem` |
| `OrderPayment` (Entity) | `ProductOrderPayment` |
| `OrderFulfillment` (AggregateRoot) | kept as-is |
| `IOrderRepository` | `IProductOrderRepository` |
| `OrderSubmittedDomainEvent` | `ProductOrderSubmittedDomainEvent` |
| `OrderSubmittedIntegrationEvent` | `ProductOrderSubmittedIntegrationEvent` |
| Table `orders` | `product_orders` |
| Table `order_items` | `product_order_items` |
| Table `order_payments` | `product_order_payments` |
| Route prefix `/api/v{version}/orders` | `/api/v{version}/productOrder` |

**Existing field preservation:** All current fields (`CustomerId`, `CustomerName`, `OrderDate`, `SubTotal`, `GrandTotal`, `Currency`, addresses, etc.) remain unchanged. Only additions shown below.

### 1.2 BillingAccount Reference (Linking WP-023)

Added to `ProductOrder`:
- `BillingAccountId` (Guid?, nullable)
- `BillingAccountHref` (string?, nullable — `/api/v1/billing/billing-accounts/{id}`)

### 1.3 TMF Attribute Alignment

Added to `ProductOrder`:
- `OrderVersion` (int, default 1) — incremented on each update
- `Priority` (enum: `Low`, `Medium`, `High`, `Critical`)
- `ProductOfferingQualificationId` (Guid?, nullable — link to WP-021 ServiceQualification)
- `ProductOfferingQualificationHref` (string?, nullable)
- `QuoteHref` (string?, nullable — alongside existing `QuoteId`)

Already present (no changes needed):
- `RequestedStartDate`, `RequestedCompletionDate`, `ExpectedCompletionDate`
- `NotificationContact`
- `RelatedParties` (sealed record `RelatedParty`)
- `AtType` ("ProductOrder"), `AtBaseType` ("ProductOrder"), `AtSchemaLocation`

### 1.4 Priority Enum

```csharp
enum Priority { Low, Medium, High, Critical }
```

Replaces the existing `string? Priority` field.

---

## 2. ProductOrderItem State Machine

### 2.1 State Enum

```csharp
enum ProductOrderItemState
{
    Acknowledged,
    InProgress,
    Pending,
    Held,
    Assessing,
    Rejected,
    Cancelled,
    Completed,
    Failed
}
```

### 2.2 State Transitions

```
Acknowledged ──startProgress()──> InProgress
Acknowledged ──cancel()──> Cancelled
InProgress ──hold()──> Held
InProgress ──assess()──> Assessing
InProgress ──pending()──> Pending
InProgress ──fail(error)──> Failed
InProgress ──cancel()──> Cancelled
Held ──resume()──> InProgress
Held ──cancel()──> Cancelled
Assessing ──reject(reason)──> Rejected
Assessing ──complete()──> Completed
Pending ──startProgress()──> InProgress
Pending ──cancel()──> Cancelled
```

All methods guard against invalid transitions by throwing `InvalidProductOrderItemStateException`.

### 2.3 Methods on `ProductOrderItem`

- `Acknowledge()` — called on item creation
- `StartProgress()` — begin active processing
- `Hold()` — pause processing
- `Resume()` — resume from hold
- `Assess()` — begin assessment (feasibility/eligibility)
- `Pending(reason)` — waiting on external dependency
- `Reject(reason)` — assessment failed
- `Complete()` — item fulfilled
- `Fail(error)` — processing error
- `Cancel()` — item cancelled

### 2.4 Order-Level Auto-Transition

When all items reach Completed, `ProductOrder.MarkCompleted()` is called automatically (via domain event or handler). When any item reaches Rejected/Failed, the order supports partial completion via a `PartiallyCompleted` meta-state exposed in the DTO (internal enum stays the same).

---

## 3. Item Relationships (Bundling)

### 3.1 Entity

```csharp
sealed record ProductOrderItemRelationship
{
    Guid Id
    Guid ProductOrderItemId         // FK — the source item
    Guid TargetItemId               // FK — the item being related to
    RelationshipType Type           // Requires, OptionalFor, ReliesOn
}
```

### 3.2 RelationshipType Enum

| Value | Meaning |
|-------|---------|
| `Requires` | Source item requires target item. If target fails, source also fails. |
| `OptionalFor` | Source item is optional for target item. Target proceeds even if source fails. |
| `ReliesOn` | Source item's progress depends on target item. Target state changes cascade to source (e.g., target held → source held). |

### 3.3 Domain Methods on `ProductOrder`

- `AddItemRelationship(itemId, targetItemId, type)` — adds relationship, validates circular dependency (A→B→C→A not allowed)
- `RemoveItemRelationship(relationshipId)` — removes relationship
- `GetItemRelationships(itemId)` — returns relationships for a specific item

### 3.4 Storage

Table `product_order_item_relationships` with FK to `product_order_items`. Cascading deletes on ProductOrderItem deletion.

---

## 4. Milestones

### 4.1 Entity

```csharp
class ProductOrderMilestone : Entity<Guid>
{
    Guid ProductOrderId
    string Name               // e.g. "OrderCreated", "OrderSubmitted"
    string Description
    DateTime MilestoneDate
    MilestoneStatus Status    // Pending, Achieved, Missed, Cancelled
}
```

### 4.2 Status Enum

```csharp
enum MilestoneStatus { Pending, Achieved, Missed, Cancelled }
```

### 4.3 Auto-Created Milestones

The `ProductOrder` domain creates milestones automatically on key lifecycle transitions:

| Transition | Milestone Name | Status |
|------------|---------------|--------|
| Construction | "OrderCreated" | Achieved |
| `Submit()` | "OrderSubmitted" | Achieved |
| `Approve()` | "OrderApproved" | Achieved |
| `CreateFulfillment()` | "FulfillmentStarted" | Achieved |
| `MarkCompleted()` | "OrderCompleted" | Achieved |

### 4.4 Custom Milestones

Exposed via API for external systems to add (e.g., "CustomerInstallVisitScheduled", "SiteSurveyCompleted").

### 4.5 Storage

Table `product_order_milestones` with FK to `product_orders`.

---

## 5. Application Layer

### 5.1 Renamed Commands (existing, renamed)

| Old Command | New Command |
|-------------|-------------|
| `CreateOrderCommand` | `CreateProductOrderCommand` |
| `AddOrderItemCommand` | `AddProductOrderItemCommand` |
| `RemoveOrderItemCommand` | `RemoveProductOrderItemCommand` |
| `SubmitOrderCommand` | `SubmitProductOrderCommand` |
| `ApproveOrderCommand` | `ApproveProductOrderCommand` |
| `CancelOrderCommand` | `CancelProductOrderCommand` |
| `DeleteOrderCommand` | `DeleteProductOrderCommand` |
| `PartialUpdateOrderCommand` | `PatchProductOrderCommand` |
| `StartOrderFulfillmentCommand` | unchanged |
| `CompleteOrderFulfillmentCommand` | unchanged |
| `ValidateOrderCommand` | `ValidateProductOrderCommand` |

### 5.2 New Commands (TMF622 additions)

| Command | Description |
|---------|-------------|
| `AcknowledgeProductOrderItemCommand` | Transition item to Acknowledged |
| `StartProductOrderItemCommand` | Transition item to InProgress |
| `HoldProductOrderItemCommand` | Transition item to Held |
| `ResumeProductOrderItemCommand` | Resume from Held |
| `AssessProductOrderItemCommand` | Transition item to Assessing |
| `RejectProductOrderItemCommand` | Transition item to Rejected |
| `CompleteProductOrderItemCommand` | Transition item to Completed |
| `FailProductOrderItemCommand` | Transition item to Failed |
| `CancelProductOrderItemCommand` | Transition item to Cancelled |
| `AddItemRelationshipCommand` | Add relationship between items |
| `RemoveItemRelationshipCommand` | Remove relationship |
| `CreateMilestoneCommand` | Add custom milestone |
| `UpdateMilestoneCommand` | Update milestone status/date |
| `RemoveMilestoneCommand` | Remove milestone |

### 5.3 Renamed Queries

| Old Query | New Query |
|-----------|-----------|
| `GetOrderByIdQuery` | `GetProductOrderByIdQuery` |
| `GetOrdersQuery` | `GetProductOrdersQuery` |
| `GetOrdersByCustomerQuery` | `GetProductOrdersByCustomerQuery` |
| `GetOrderFulfillmentStatusQuery` | unchanged |

### 5.4 New Queries

| Query | Description |
|-------|-------------|
| `GetProductOrderItemRelationshipsQuery` | List relationships for an item |
| `GetProductOrderMilestonesQuery` | List milestones for an order |

### 5.5 Renamed DTOs

| Old DTO | New DTO |
|---------|---------|
| `OrderDto` | `ProductOrderDto` |
| `OrderItemDto` | `ProductOrderItemDto` |
| `OrderSummaryDto` | `ProductOrderSummaryDto` |
| `OrderPaymentDto` | `ProductOrderPaymentDto` |
| `OrderFulfillmentDto` | unchanged |
| `OrderValidationResultDto` | `ProductOrderValidationResultDto` |

### 5.6 New DTOs

- `ProductOrderItemRelationshipDto`
- `ProductOrderMilestoneDto`
- `ProductOrderItemPriceDto` — structured pricing (one-time, recurring, discount, tax)

### 5.7 Renamed Domain Events

| Old Event | New Event |
|-----------|-----------|
| `OrderSubmittedDomainEvent` | `ProductOrderSubmittedDomainEvent` |
| `OrderApprovedDomainEvent` | `ProductOrderApprovedDomainEvent` |
| `OrderCancelledDomainEvent` | `ProductOrderCancelledDomainEvent` |
| `OrderCompletedDomainEvent` | `ProductOrderCompletedDomainEvent` |

### 5.8 New Domain Events

- `ProductOrderItemStateChangedDomainEvent` — item state transitioned (carries old/new state, item id, order id)
- `ProductOrderMilestoneReachedDomainEvent` — milestone achieved or missed

### 5.9 Renamed Integration Events

| Old Event | New Event |
|-----------|-----------|
| `OrderSubmittedIntegrationEvent` | `ProductOrderSubmittedIntegrationEvent` |
| `OrderApprovedIntegrationEvent` | `ProductOrderApprovedIntegrationEvent` |
| `OrderFulfillmentStartedIntegrationEvent` | unchanged |
| `ProvisioningRequiredIntegrationEvent` | unchanged |
| `SubscriptionRequiredIntegrationEvent` | unchanged |

### 5.10 New Integration Events

- `ProductOrderItemStateChangedIntegrationEvent` — published when item state changes
- `ProductOrderMilestoneReachedIntegrationEvent` — published when milestone is achieved

### 5.11 Updated Event Consumers

- `QuoteAcceptedIntegrationEventHandler` (in Orders) — creates `ProductOrder` instead of `Order`
- `OrderApprovedIntegrationEventHandler` (self) — renamed internally
- `OrderSubmittedEventHandler` (self) — renamed internally
- Cross-module consumers of renamed events (Provisioning, Subscriptions) — update event type references

---

## 6. Infrastructure Layer

### 6.1 EF Configurations

**New/updated configs:**
- `ProductOrderConfiguration` — renamed from `OrderConfiguration`, table `product_orders`. Added `BillingAccountId`, `BillingAccountHref`, `OrderVersion`, `Priority` (stored as string), `ProductOfferingQualificationId`, `ProductOfferingQualificationHref`, `QuoteHref`
- `ProductOrderItemConfiguration` — renamed from `OrderItemConfiguration`, table `product_order_items`. Added `State` (stored as string)
- `ProductOrderPaymentConfiguration` — renamed from `OrderPaymentConfiguration`, table `product_order_payments`
- `ProductOrderItemRelationshipConfiguration` — new, table `product_order_item_relationships`
- `ProductOrderMilestoneConfiguration` — new, table `product_order_milestones`
- `OrderFulfillmentConfiguration` — unchanged

### 6.2 Migration

A single migration `RenameAndAddTmf622Resources` that:
1. Renames tables: `orders` → `product_orders`, `order_items` → `product_order_items`, `order_payments` → `product_order_payments`
2. Adds columns: `billing_account_id`, `billing_account_href`, `order_version`, `priority`, `product_offering_qualification_id`, `product_offering_qualification_href`, `quote_href` to `product_orders`
3. Adds `state` column to `product_order_items` (default `Acknowledged`)
4. Creates new tables: `product_order_item_relationships`, `product_order_milestones`

### 6.3 Repository Updates

- `OrderRepository` → `ProductOrderRepository` — renamed, updated queries
- `IOrderFulfillmentRepository` unchanged

---

## 7. API Layer

### 7.1 Route Changes

Base route changes from `/api/v{version}/orders` to `/api/v{version}/productOrder`.

### 7.2 Renamed Endpoints

| Old Route | New Route | Method |
|-----------|-----------|--------|
| `/orders` | `/productOrder` | POST (create) |
| `/orders/{id}` | `/productOrder/{id}` | GET (detail) |
| `/orders` | `/productOrder` | GET (list) |
| `/orders/{id}` | `/productOrder/{id}` | PATCH |
| `/orders/{id}` | `/productOrder/{id}` | DELETE |
| `/orders/{id}/submit` | `/productOrder/{id}/submit` | POST |
| `/orders/{id}/approve` | `/productOrder/{id}/approve` | POST |
| `/orders/{id}/cancel` | `/productOrder/{id}/cancel` | POST |
| `/orders/{id}/items` | `/productOrder/{id}/items` | POST |
| `/orders/{orderId}/items/{itemId}` | `/productOrder/{orderId}/items/{itemId}` | DELETE |
| `/orders/{id}/validate` | `/productOrder/{id}/validate` | POST |

### 7.3 New Endpoints

| Route | Method | Description |
|-------|--------|-------------|
| `/productOrder/{id}/items/{itemId}/acknowledge` | POST | Acknowledge item |
| `/productOrder/{id}/items/{itemId}/start` | POST | Start item processing |
| `/productOrder/{id}/items/{itemId}/hold` | POST | Hold item |
| `/productOrder/{id}/items/{itemId}/resume` | POST | Resume item |
| `/productOrder/{id}/items/{itemId}/assess` | POST | Assess item |
| `/productOrder/{id}/items/{itemId}/reject` | POST | Reject item |
| `/productOrder/{id}/items/{itemId}/complete` | POST | Complete item |
| `/productOrder/{id}/items/{itemId}/fail` | POST | Fail item |
| `/productOrder/{id}/items/{itemId}/cancel` | POST | Cancel item |
| `/productOrder/{id}/relationships` | GET | List relationships |
| `/productOrder/{id}/relationships` | POST | Add relationship |
| `/productOrder/{id}/relationships/{relationshipId}` | DELETE | Remove relationship |
| `/productOrder/{id}/milestones` | GET | List milestones |
| `/productOrder/{id}/milestones` | POST | Create custom milestone |
| `/productOrder/{id}/milestones/{milestoneId}` | PATCH | Update milestone |
| `/productOrder/{id}/milestones/{milestoneId}` | DELETE | Remove milestone |

All new endpoints follow the existing patterns (Mapster, FluentValidation, MediatR, Result<T> return type).

---

## 8. Frontend

### 8.1 Frontend Route Changes

- `/orders` → `/product-order` for all page routes (list, new, detail, edit)
- Old `/orders/*` routes redirect to `/product-order/*`

### 8.2 Updates to Existing Pages

- Rename all references from `Order` → `ProductOrder` in API hooks, DTOs, query keys, and component props
- Add BillingAccount selector/display to create and edit forms (dropdown fetching from `/api/v1/billing/billing-accounts`)
- Add Priority field (select: Low/Medium/High/Critical) to create/edit forms
- Add OrderVersion display (read-only badge on detail page)

### 8.3 New Pages/Components

- **Order Item State Timeline** — visual component on the detail page showing state history of each item with transition buttons
- **Relationships Panel** — on the detail page, list/add/remove item relationships
- **Milestones Panel** — on the detail page, timeline view of milestones with status badges

### 8.3 New Query Keys

```typescript
productOrder: {
  all: [...],
  lists: () => [...],
  list: (filters) => [...],
  details: () => [...],
  detail: (id) => [...],
  relationships: (id) => [...productOrder.detail(id), "relationships"],
  milestones: (id) => [...productOrder.detail(id), "milestones"],
  itemStates: (id) => [...productOrder.detail(id), "item-states"],
}
```

---

## 9. Testing

### 9.1 Unit Tests (Orders.Tests)

- Domain tests for `ProductOrderItem` state machine (all valid + invalid transitions)
- Domain tests for `ProductOrder` item relationship validation (circular dependency detection)
- Domain tests for milestone auto-creation on lifecycle transitions
- Handler tests for all new commands (item state transitions, relationships, milestones)

### 9.2 Integration Tests

- Integration test for full order lifecycle with item state tracking
- Integration test for relationship CRUD with persistence
- Integration test for milestone CRUD with persistence

---

## 10. Migration & Backward Compatibility

### 10.1 Data Migration

- Rename tables: `orders` → `product_orders`, `order_items` → `product_order_items`, `order_payments` → `product_order_payments`
- Existing rows get `order_version = 1`, `priority = 'Medium'`, `state = 'Acknowledged'` defaults
- Existing related party data is preserved in the renamed table structure
- No data loss expected

### 10.2 API Compatibility

- Old path `/api/v{version}/orders` returns 301 redirect to `/api/v{version}/productOrder`
- Integration event type names change — consumers must be updated in the same deployment

---

## 11. Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Rename Order → ProductOrder? | Yes | User chose full TMF622 compliance over backward compatibility |
| Item state as enum column on existing table | Yes | Simple, queryable, no separate state history table needed |
| Relationships as separate table | Yes | Queryable independently, supports FK validation |
| Milestones as separate table | Yes | Custom milestones added via API, not just auto-created |
| Priority replaces string field | Yes | Type safety, aligns with TMF622 typed enum |
| Fulfillment kept separate aggregate | Yes | Already well-structured, no TMF622 conflict |
