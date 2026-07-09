# WP-020: Quote Management (TMF648)

**Goal:** Add a TMF648-compliant `Quote` aggregate root to the CRM module, enabling sales/pre-ordering quote creation, lifecycle management, and pricing. Quotes feed into the existing Ordering flow once accepted.

**Architecture:** New `Quote` aggregate root in the CRM module, alongside existing `Customer` and `Agreement`. Reuses existing CRM value objects (`RelatedParty`, `AccountRef`, `AgreementRef`, `ContactMedium`, `CharValue`) via project reference. No structural changes to existing aggregates.

---

## Module Placement: CRM

**Rationale:** Quote Management sits in the TM Forum Customer domain (same as Customer/Party/Agreement). Quotes reference Customer as a related party, Agreements for framework terms, and Billing Accounts for pricing context. Placing Quote in the CRM module avoids cross-module coupling and reuses mature value objects without duplication. If concerns arise about CRM scope, Quote can be extracted to a standalone Sales module later.

---

## Domain Model

### Quote (AggregateRoot<Guid>)

| Field | Type | Notes |
|-------|------|-------|
| `Id` | Guid | PK |
| `TenantId` | Guid | Multi-tenant |
| `ExternalId` | string? | Consumer-provided ID |
| `State` | QuoteState | Lifecycle (see below) |
| `QuoteDate` | DateTime | Created timestamp |
| `Category` | string? | e.g. "enterprise", "residential" |
| `Description` | string? | Free text |
| `Version` | int | Increments on renegotiation |
| `ValidFrom` | DateTime? | Validity window start |
| `ValidUntil` | DateTime? | Validity window end |
| `ExpectedQuoteCompletionDate` | DateTime? | Requested response date |
| `EffectiveQuoteCompletionDate` | DateTime? | Actual completion |
| `ExpectedFulfillmentStartDate` | DateTime? | Desired delivery date |
| `CustomerId` | Guid | FK to Customer (primary party) |
| `CreatedAt` | DateTime | |
| `UpdatedAt` | DateTime | |

**Collections:**
- `QuoteItem[]` — line items (mandatory on create)
- `RelatedParty[]` — uses CRM's existing `RelatedParty` value object
- `QuotePrice[]` — total price aggregation (value object)
- `QuoteAuthorization[]` — approval records (value object)
- `BillingAccountRef[]` — uses CRM's existing `AccountRef`
- `AgreementRef[]` — uses CRM's existing `AgreementRef`
- `Note[]` — annotations (value object)

### QuoteItem (Entity<Guid>)

| Field | Type | Notes |
|-------|------|-------|
| `Id` | Guid | PK |
| `Action` | QuoteItemAction | Add, Modify, Delete, NoChange |
| `State` | QuoteItemState | InProgress → Pending → Approved/Rejected |
| `Quantity` | int | |
| `ProductOfferingId` | Guid? | Ref to ProductCatalog |
| `ProductOfferingName` | string? | Denormalized for display |
| `ProductId` | Guid? | Ref to existing Product (for modify/delete) |
| `CreatedAt` | DateTime | |
| `UpdatedAt` | DateTime | |

**Collections:**
- `QuoteItemPrice[]` — per-item pricing (value object)
- `QuoteItemRelationship[]` — item-to-item links (value object)
- `Note[]` — per-item notes (value object)

### Enums

**QuoteState:**
- `InProgress` — initial state
- `Pending` — ready for review/approval
- `Approved` — pricing approved internally
- `Accepted` — customer accepted the quote
- `Rejected` — customer rejected
- `Cancelled` — withdrawn

**QuoteItemAction:**
- `Add` — new product/offering
- `Modify` — change existing product
- `Delete` — remove existing product
- `NoChange` — retain as-is

**QuoteItemState:**
- `InProgress`
- `Pending`
- `Approved`
- `Rejected`

### Value Objects

**QuotePrice:**
```
QuotePrice(PriceType priceType, string name, decimal dutyFreeAmount,
           decimal taxIncludedAmount, decimal taxRate, string currency,
           string? unitOfMeasure, int? recurringPeriod, string? recurringPeriodUnit)
```
Where `PriceType` enum: `OneTime`, `Recurring`, `Usage`

**PriceAlteration** (discounts/adjustments on a price):
```
PriceAlteration(string name, string? description, PriceType priceType,
                int? applicationDuration, int priority, decimal dutyFreeAmount,
                decimal taxIncludedAmount, decimal taxRate, string currency)
```

**QuoteAuthorization:**
```
QuoteAuthorization(AuthorizationState state, DateTime requestedDate,
                   DateTime? givenDate, RelatedParty? approver)
```
Where `AuthorizationState` enum: `Approved`, `Declined`

**QuoteItemRelationship:**
```
QuoteItemRelationship(Guid itemId, Guid relatedItemId, string type)
```

**Note:**
```
Note(DateTime date, string author, string text)
```

---

## Domain Events

- `QuoteCreatedDomainEvent` (QuoteId, CustomerId)
- `QuoteStateChangedDomainEvent` (QuoteId, OldState, NewState)
- `QuoteAcceptedDomainEvent` (QuoteId, CustomerId) — triggers Order creation
- `QuoteItemStateChangedDomainEvent` (QuoteId, ItemId, OldState, NewState)

---

## Application Layer

### Repository Interface

```
IQuoteRepository
  GetByIdAsync(id)
  GetListAsync(filter)       — filterable by state, customerId, date range
  GetByCustomerAsync(customerId)
  AddAsync(quote)
  UpdateAsync(quote)
  DeleteAsync(quote)
  CountAsync(filter)
```

### DTOs

- `QuoteDto` — flat DTO with all child collections as lists
- `QuoteItemDto`
- `QuotePriceDto`
- `PriceAlterationDto`
- `QuoteAuthorizationDto`
- `QuoteItemRelationshipDto`
- `NoteDto`

Mapster mapping in `CrmMappingConfig.cs` following existing patterns (enum→string, nested VO→DTO).

### Commands

| Command | Action |
|---------|--------|
| `CreateQuoteCommand` | Create quote with items, parties, pricing |
| `UpdateQuoteCommand` | Update quote details (description, category, dates) |
| `SubmitQuoteCommand` | Move to Pending state |
| `ApproveQuoteCommand` | Approve internally (→ Approved) |
| `AcceptQuoteCommand` | Customer accepts (→ Accepted) |
| `RejectQuoteCommand` | Customer rejects (→ Rejected) |
| `CancelQuoteCommand` | Cancel/withdraw (→ Cancelled) |
| `AddQuoteItemCommand` | Add item to existing quote |
| `UpdateQuoteItemCommand` | Modify item details |
| `RemoveQuoteItemCommand` | Remove item from quote |

### Queries

| Query | Returns |
|-------|---------|
| `GetQuoteByIdQuery` | QuoteDto |
| `GetQuotesQuery` | Paginated list, filterable |
| `GetQuotesByCustomerQuery` | List for a customer |

### Integration Events (outbound)

- `QuoteCreatedIntegrationEvent` — for CRM notification, audit
- `QuoteAcceptedIntegrationEvent` — consumed by Orders module to create Order
- `QuoteStateChangedIntegrationEvent` — broadcast state transitions

### Event Handler (inbound)

- `CustomerCreatedEventHandler` — (future) auto-create welcome quote

---

## Infrastructure

### EF Configuration

- `QuoteConfiguration` — table `quotes`, owned value objects, HasMany for QuoteItems
- `QuoteItemConfiguration` — table `quote_items`, owned value objects

Owned value objects (`QuotePrice`, `PriceAlteration`, `QuoteAuthorization`, `QuoteItemRelationship`, `Note`) stored as JSON columns or separate tables based on query patterns. Given the nested nature, **JSON columns** in PostgreSQL are preferred (e.g., `quote_prices jsonb`, `quote_authorizations jsonb`).

**Tables created:**
- `quotes` — Quote aggregate root
- `quote_items` — QuoteItem child entities

**JSON columns on `quotes`:**
- `related_parties` (jsonb)
- `quote_prices` (jsonb)
- `quote_authorizations` (jsonb)
- `billing_account_refs` (jsonb)
- `agreement_refs` (jsonb)
- `notes` (jsonb)

**JSON columns on `quote_items`:**
- `prices` (jsonb)
- `item_relationships` (jsonb)
- `notes` (jsonb)

### Repository

`QuoteRepository` — implements `IQuoteRepository`, uses EF Core with JSON column queries where needed.

### Migration

`AddQuoteManagementTables` — creates `quotes` and `quote_items` tables.

---

## API Endpoints (under `/api/v1/crm`)

| Method | Path | Action |
|--------|------|--------|
| `POST` | `/quotes` | Create quote |
| `GET` | `/quotes/{id}` | Get by ID |
| `GET` | `/quotes` | List (paginated, filterable) |
| `PATCH` | `/quotes/{id}` | Update details |
| `POST` | `/quotes/{id}/submit` | Submit for approval |
| `POST` | `/quotes/{id}/approve` | Internal approval |
| `POST` | `/quotes/{id}/accept` | Customer acceptance |
| `POST` | `/quotes/{id}/reject` | Customer rejection |
| `POST` | `/quotes/{id}/cancel` | Cancel |
| `POST` | `/quotes/{id}/items` | Add item |
| `PATCH` | `/quotes/{id}/items/{itemId}` | Update item |
| `DELETE` | `/quotes/{id}/items/{itemId}` | Remove item |
| `GET` | `/customers/{customerId}/quotes` | By customer |

---

## Integration with Ordering

When a Quote transitions to `Accepted` state:
1. `QuoteAcceptedDomainEvent` raised
2. `QuoteAcceptedIntegrationEvent` published
3. Orders module handler creates an Order from the accepted Quote's items
4. Quote items with `Action = Add` become Order items referencing the ProductOffering

**Data flow:** `Quote → Order → Product Inventory (via provisioning)`

---

## Scope

### Includes
- Quote CRUD with lifecycle management
- Quote item management (add/update/remove items)
- Pricing support (per-item and total)
- Validation of state transitions
- Integration events for Order creation on acceptance
- List/filter quotes by customer, state, date range
- Frontend: Quotes list and detail pages under CRM section

### Not In Scope
- Quote authorization/approval workflow engine (simple state machine only)
- Renegotiation versioning (version field exists but auto-increment not in MVP)
- Appointment scheduling
- Attachment/file upload support
- `QuoteInformationRequiredEvent` notification (future)
- Advanced pricing engine/calculations (prices are input, not computed)

---

## TMF648 Compliance Notes

- Quote lifecycle follows TMF648 v4.0 state machine: `InProgress → Pending → Approved | Accepted | Rejected | Cancelled`
- `quoteItem` is mandatory per spec — create command requires at least one item
- `relatedParty` at quote level for customer/sales rep; `relatedParty` at item level for per-item roles
- Price model uses the `QuotePrice` structure with `dutyFreeAmount`, `taxIncludedAmount`, and `taxRate` per TMF spec
- `PriceAlteration` supports discount modeling per TMF648
- Authorization tracking follows the spec's `quoteAuthorization` array pattern
- `validFor` modeled as `ValidFrom`/`ValidUntil` on Quote (spec uses TimePeriod)
