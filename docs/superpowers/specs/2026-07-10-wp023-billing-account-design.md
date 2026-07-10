# WP-023: Billing Account (TMF666) — Design Spec

**Date:** 2026-07-10
**Module:** `Obss.Billing` (enhancement)
**TMF API:** TMF666 Account Management
**Approach:** Full TMF666 Resource Expansion (Approach 1)

---

## 1. Domain Model

### 1.1 New Entities

**`AccountBalance`** (AggregateRoot\<Guid>)
- `BillingAccountId` (Guid) — FK to billing account
- `CurrentBalance` (decimal) — net balance (positive = customer owes)
- `OutstandingBalance` (decimal) — overdue/unpaid amount
- `AvailableCredit` (decimal) — credit limit minus outstanding
- `Currency` (string, 3)
- `BalanceDate` (DateTime) — effective date of this snapshot
- `LastUpdatedAt` (DateTime)
- `AtType` (string?, default "AccountBalance")
- `AtBaseType` (string?, default "PartyBalance")
- `AtSchemaLocation` (string?)
- Collections: `Transactions` (List\<BalanceTransaction>)

**`BalanceTransaction`** (Entity\<Guid>)
- `BalanceId` (Guid) — FK to AccountBalance
- `Amount` (decimal) — signed (negative = debit/payment)
- `TransactionType` (enum: Charge, Payment, Credit, Debit, Adjustment, Refund)
- `Description` (string)
- `TransactionDate` (DateTime)
- `ReferenceId` (string?) — link to bill, payment, or adjustment ID
- `ReferenceType` (string?) — "Bill", "Payment", "Adjustment"

### 1.2 Owned Entities on BillingAccount

**`BillPresentationMedia`** (owned, multiple)
- `MediaType` (string — "Email", "Paper", "Portal")
- `EmailAddress` (string?)
- `PaperFormat` (string? — "A4", "Letter")
- `Language` (string, default "en")
- `IsPreferred` (bool)
- `ValidFrom` (DateTime?)
- `ValidUntil` (DateTime?)

### 1.3 Value Objects

**`AccountHolder`** (owned, single)
- `Name` (string, 200)
- `Email` (string, 200?)
- `Phone` (string, 50?)
- `ContactId` (Guid?) — optional link to CRM Contact

### 1.4 Existing Entity Enhancements

**`BillingAccount`** — add:
- `AccountHolder` (AccountHolder?, OwnsOne)
- `BillPresentationMedia` (List\<BillPresentationMedia>, OwnsMany)
- `PaymentMethodId` (string?, max 100 — gateway token ref)

**`TaxExemption`** — add:
- `BillingAccountId` (Guid?, nullable FK) — links exemption to a specific billing account

### 1.5 New Domain Events

- `BillingAccountCreatedEvent` — fired after billing account creation
- `BillingAccountUpdatedEvent` — fired after update/patch
- `BillingAccountDeletedEvent` — fired after soft delete
- `BalanceChangedEvent` — fired when balance changes (with previous/new values)

### 1.6 Enums

- `TransactionType`: `Charge`, `Payment`, `Credit`, `Debit`, `Adjustment`, `Refund`
- `MediaType`: `Email`, `Paper`, `Portal`

---

## 2. Application Layer

### 2.1 New Commands

| Command | Returns | Endpoint |
|---------|---------|----------|
| `PatchBillingAccountCommand` | `Result<BillingAccountDto>` | PATCH `/billing-accounts/{id}` |
| `DeleteBillingAccountCommand` | `Result` | DELETE `/billing-accounts/{id}` |
| `AddBillingAccountRelatedPartyCommand` | `Result<BillingAccountDto>` | POST `/billing-accounts/{id}/related-parties` |
| `RemoveBillingAccountRelatedPartyCommand` | `Result` | DELETE `/billing-accounts/{id}/related-parties/{partyId}` |
| `CreateBillPresentationMediaCommand` | `Result<BillPresentationMediaDto>` | POST `/billing-accounts/{id}/presentation-media` |
| `UpdateBillPresentationMediaCommand` | `Result<BillPresentationMediaDto>` | PUT `/billing-accounts/{id}/presentation-media/{mediaId}` |
| `RemoveBillPresentationMediaCommand` | `Result` | DELETE `/billing-accounts/{id}/presentation-media/{mediaId}` |
| `RecordBalanceTransactionCommand` | `Result` | POST `/billing-accounts/{id}/adjustments` |

### 2.2 New Queries

| Query | Returns | Endpoint |
|-------|---------|----------|
| `GetBillingAccountBalanceQuery` | `Result<AccountBalanceDto>` | GET `/billing-accounts/{id}/balance` |
| `GetBillingAccountRelatedPartiesQuery` | `Result<List<RelatedPartyDto>>` | GET `/billing-accounts/{id}/related-parties` |
| `GetBillingAccountBillPresentationMediaQuery` | `Result<List<BillPresentationMediaDto>>` | GET `/billing-accounts/{id}/presentation-media` |

### 2.3 New DTOs

- `AccountBalanceDto` — Id, BillingAccountId, CurrentBalance, OutstandingBalance, AvailableCredit, Currency, BalanceDate
- `BalanceTransactionDto` — Id, Amount, TransactionType, Description, TransactionDate, ReferenceId, ReferenceType
- `BillPresentationMediaDto` — Id, MediaType, EmailAddress, PaperFormat, Language, IsPreferred
- `AccountHolderDto` — Name, Email?, Phone?, ContactId?
- `PatchBillingAccountRequest` — all fields nullable for partial update

### 2.4 Integration Events

- `BillingAccountCreatedIntegrationEvent` — published on create (BillingAccountId, CustomerId, AccountType, timestamp)
- `BillingAccountUpdatedIntegrationEvent` — published on update/patch
- `BillingAccountDeletedIntegrationEvent` — published on soft delete
- `BillingAccountBalanceChangedIntegrationEvent` — published on balance change (BillingAccountId, PreviousBalance, NewBalance, timestamp)

All follow the existing `BillFinalizedEventHandler` pattern: DomainEvent → Handler → Outbox → MassTransit.

### 2.5 Mapster Mapping

- Existing `BillingMappingConfig` enhanced with:
  - `AccountBalance` → `AccountBalanceDto`
  - `BalanceTransaction` → `BalanceTransactionDto`
  - `BillPresentationMedia` → `BillPresentationMediaDto`
  - `AccountHolder` → `AccountHolderDto`

---

## 3. Infrastructure Layer

### 3.1 EF Configurations

**`AccountBalanceConfiguration`** — table: `account_balances`
- All properties mapped with snake_case
- `OwnsMany(b => b.Transactions)` → `balance_transactions` table, cascade delete
- `HasIndex(b => b.BillingAccountId)` for lookup queries

**`BalanceTransactionConfiguration`** — table: `balance_transactions`
- All properties mapped with snake_case
- TransactionType stored as string (max 20)
- FK `balance_id` → `account_balances.id` cascade

**BillingAccount enhancement** — existing `BillingAccountConfiguration`:
- `OwnsOne(b => b.AccountHolder)` → `billing_accounts` inline columns (account_holder_name, account_holder_email, account_holder_phone, account_holder_contact_id)
- `OwnsMany(b => b.BillPresentationMedia)` → `billing_account_presentation_media` table
- Add `PaymentMethodId` column to billing_accounts

**TaxExemption enhancement** — existing `TaxExemptionConfiguration`:
- Add `BillingAccountId` nullable column, FK to `billing_accounts.id`

### 3.2 Repositories

**`IAccountBalanceRepository` / `AccountBalanceRepository`**
- `GetByBillingAccountIdAsync(Guid billingAccountId)` — current balance
- `GetByBillingAccountIdWithTransactionsAsync(Guid billingAccountId)` — balance + transactions
- `GetByBillingAccountIdAsOfAsync(Guid billingAccountId, DateTime asOf)` — point-in-time balance

### 3.3 Migration

- New migration: `AddBillingAccountTmf666Resources`
- Creates tables: `account_balances`, `balance_transactions`, `billing_account_presentation_media`
- Alters tables: `billing_accounts` (add columns), `tax_exemptions` (add BillingAccountId FK)

---

## 4. API Layer

### 4.1 New Endpoints

All under `api/v{version}/billing` route group, added to existing `BillingEndpoints.cs`:

| Method | Path | Handler |
|--------|------|---------|
| PATCH | `/billing-accounts/{id}` | PatchBillingAccount |
| DELETE | `/billing-accounts/{id}` | DeleteBillingAccount |
| GET | `/billing-accounts/{id}/balance` | GetBillingAccountBalance |
| POST | `/billing-accounts/{id}/adjustments` | RecordBalanceTransaction |
| GET | `/billing-accounts/{id}/related-parties` | GetBillingAccountRelatedParties |
| POST | `/billing-accounts/{id}/related-parties` | AddBillingAccountRelatedParty |
| DELETE | `/billing-accounts/{id}/related-parties/{partyId}` | RemoveBillingAccountRelatedParty |
| GET | `/billing-accounts/{id}/presentation-media` | GetBillingAccountBillPresentationMedia |
| POST | `/billing-accounts/{id}/presentation-media` | CreateBillPresentationMedia |
| PUT | `/billing-accounts/{id}/presentation-media/{mediaId}` | UpdateBillPresentationMedia |
| DELETE | `/billing-accounts/{id}/presentation-media/{mediaId}` | RemoveBillPresentationMedia |

### 4.2 Module Registration

Existing `BillingModuleRegistration.cs` updated — no new service registrations needed beyond the new repositories (already covered by assembly scanning convention).

---

## 5. Frontend

### 5.1 Pages

| Route | Content |
|-------|---------|
| `/billing/accounts` | Billing account list: search/filter by status, type, customer name. Columns: Name, Type, Status, Balance, Currency, Customer |
| `/billing/accounts/new` | Create form: customer selector (search), account type, name, credit limit, currency, account holder fields (name, email, phone) |
| `/billing/accounts/[id]` | Detail page with tabs/sections: (1) Account info card, (2) Balance widget (current/outstanding/available), (3) Related parties table with add/remove, (4) Presentation media list with add/edit/remove, (5) Tax exemptions list |
| `/billing/accounts/[id]/edit` | Edit form (same layout as new, pre-filled) |

### 5.2 Hooks

- `useBillingAccounts(filters?)` — list with search/filter
- `useBillingAccount(id)` — single account detail
- `useBillingAccountBalance(id)` — balance + transactions
- `useRelatedParties(accountId)` — related parties CRUD
- `useBillPresentationMedia(accountId)` — presentation media CRUD
- `useCreateBillingAccount()`, `useUpdateBillingAccount()`, `useDeleteBillingAccount()` — mutations
- `usePatchBillingAccount()` — partial update mutation

### 5.3 Query Keys

Add `billingAccounts` factory to existing `query-keys.ts`:
```typescript
billingAccounts: {
  all: ['billing-accounts'] as const,
  list: (filters?: Record<string, unknown>) => ['billing-accounts', 'list', filters],
  detail: (id: string) => ['billing-accounts', 'detail', id],
  balance: (id: string) => ['billing-accounts', 'balance', id],
  relatedParties: (id: string) => ['billing-accounts', id, 'related-parties'],
  presentationMedia: (id: string) => ['billing-accounts', id, 'presentation-media'],
}
```

---

## 6. Error Handling

- `BillingAccountNotFoundException` — 404 for missing billing account (exists already)
- `BalanceNotFoundException` — 404 if no balance record
- `MediaNotFoundException` — 404 for presentation media operations
- All follow existing Result pattern with domain errors
- Validation via FluentValidation on all commands (existing pattern)
