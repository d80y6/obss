# TMF629 Customer Management — Sub-projects 2, 4, 5 Design

## Objective
Extend the OBSS Customer entity with ContactMedium, AccountRef/AgreementRef/PaymentMethodRef, and NotificationHub to complete TMF629 v5.0.1 alignment.

## Architecture
Three additive sub-projects executed sequentially, each following the same pattern as Sub-project 1 (value objects/entities → EF configs → DTOs → mappings → commands/queries → endpoints → frontend types → frontend pages).

## Tech Stack
Same as Sub-project 1: .NET 9, EF Core/Npgsql, MediatR, FluentValidation, Mapster, Next.js 16, React Query, zod.

---

## Sub-project 2: ContactMedium

### Scope
Add ContactMedium value object as an owned collection on Customer, Individual, and Organization entities.

### ContactMedium Value Object
```
ContactMedium
├── Id: int (shadow PK, auto-increment)
├── MediumType: enum { Email, SMS, Phone, PostalAddress, SocialMedia, Web, Chat }
├── IsPreferred: bool
├── ValidFrom: DateTime? (UTC)
├── ValidUntil: DateTime? (UTC)
├── MediumCharacteristics -> List<ContactCharValue> (owned)
│   └── ContactCharValue { Key: string, Value: string, ValueType: string }
├── CustomerId: Guid (FK, if on Customer)
├── IndividualId: Guid? (FK, if on Individual)
├── OrganizationId: Guid? (FK, if on Organization)
```

### Table
`customer_contact_media` — owned collection on Customer with shadow FK `customer_id`

### Files Modified/Created
- Add `ContactMediumType` enum to `Obss.CRM.Domain.ValueObjects`
- Add `ContactCharValue` value object to `Obss.CRM.Domain.ValueObjects`
- Add `ContactMedium` value object to `Obss.CRM.Domain.ValueObjects`
- Add `_contactMedia` collection + `ContactMedia` property on Customer
- Add `AddContactMedium()`, `RemoveContactMedium()` methods on Customer
- Add `ContactMediumConfiguration` (owned table)
- Update `CustomerConfiguration` with contact media collection
- Add `ContactMediumDto` to `Obss.CRM.Application.DTOs`
- Add Mapster mapping
- Add commands: `AddContactMediumCommand`, `RemoveContactMediumCommand`
- Add command handlers + validators
- Add endpoints: `POST /customers/{id}/contact-media`, `DELETE /customers/{id}/contact-media/{mediumId}`
- Add `ContactMediumDto` frontend type
- Update customer detail page with Contact Media table section
- Database migration for `customer_contact_media` table

### Non-Goals
- Adding ContactMedium to Individual/Organization entities (deferred — can reuse the same value object)
- Notification preferences integration

---

## Sub-project 4: AccountRef / AgreementRef / PaymentMethodRef

### Scope
Add three reference collections on Customer referencing BillingAccount (Billing module), Agreement (CRM module), and PaymentMethod (Payments module). Create BillingAccount aggregate (Billing module) and Agreement aggregate (CRM module).

### Part A: BillingAccount (in Billing module)

#### Entity
```
BillingAccount : AggregateRoot<Guid>
├── CustomerId: Guid
├── AccountType: enum { Standard, Prepaid, Postpaid, Corporate }
├── Name: string
├── Status: string ("Active", "Suspended", "Closed")
├── CreditLimit: decimal
├── Currency: string (default "USD")
├── ValidFrom: DateTime? (UTC)
├── ValidUntil: DateTime? (UTC)
├── Description: string? (500)
├── IsActive: bool
├── CreatedAt: DateTime (UTC)
├── UpdatedAt: DateTime (UTC)
```

#### Files Created/Modified (Billing module)
- Create `Obss.Billing.Domain.Entities.BillingAccount` (aggregate root)
- Add `AccountType` enum to `Obss.Billing.Domain.ValueObjects`
- Add `BillingAccountConfiguration` to `Obss.Billing.Infrastructure.Persistence.Configurations`
- Add `DbSet<BillingAccount> BillingAccounts` to `BillingDbContext`
- Add `BillingAccountDto` to `Obss.Billing.Application.DTOs`
- Add Mapster mapping
- Add commands: `CreateBillingAccountCommand`, `UpdateBillingAccountCommand`
- Add queries: `GetBillingAccountByIdQuery`, `SearchBillingAccountsQuery`
- Add endpoints to `BillingEndpoints` (POST, GET /{id}, PUT /{id}, GET)
- Register `IRepository<BillingAccount>` in Billing module registration
- Add frontend `BillingAccountDto` type
- Database migration for `billing_accounts` table

### Part B: Agreement (in CRM module)

#### Entity
```
Agreement : AggregateRoot<Guid>
├── CustomerId: Guid
├── Name: string (200)
├── AgreementType: enum { TermsOfService, SLA, Contract, Addendum }
├── Status: string ("Active", "Pending", "Expired", "Terminated")
├── ValidFrom: DateTime? (UTC)
├── ValidUntil: DateTime? (UTC)
├── Description: string? (500)
├── SignedAt: DateTime? (UTC)
├── SignedBy: string? (100)
├── CreatedAt: DateTime (UTC)
├── UpdatedAt: DateTime (UTC)
```

#### Files Created/Modified (CRM module)
- Create `Obss.CRM.Domain.Entities.Agreement`
- Add `AgreementType` enum to `Obss.CRM.Domain.ValueObjects`
- Add `AgreementConfiguration` to configs
- Add `DbSet<Agreement> Agreements` to `CrmDbContext`
- Add `AgreementDto` to DTOs
- Add Mapster mapping
- Add commands: `CreateAgreementCommand`, `UpdateAgreementCommand`
- Add queries: `GetAgreementByIdQuery`, `SearchAgreementsQuery`
- Add endpoints to `CustomerEndpoints` or new `AgreementEndpoints`
- Add frontend `AgreementDto` type

### Part C: Reference Value Objects on Customer

#### Value Objects
```
AccountRef
├── BillingAccountId: Guid
├── Name: string
├── AccountType: string
├── Role: string ("Primary", "Secondary", "Temporary")
├── Href: string?

AgreementRef
├── AgreementId: Guid
├── Name: string
├── AgreementType: string
├── Role: string ("Primary", "Secondary")
├── Href: string?

PaymentMethodRef
├── PaymentMethodId: Guid
├── Name: string
├── Href: string?
```

#### Tables (owned collections on Customer)
- `customer_account_refs` — FK to `customers`
- `customer_agreement_refs` — FK to `customers`
- `customer_payment_method_refs` — FK to `customers`

#### Customer entity updates
- Add `_accountRefs`, `_agreementRefs`, `_paymentMethodRefs` collections
- Add `AddAccountRef()`, `RemoveAccountRef()` methods (same pattern for agreements and payment methods)

#### Files Modified
- CustomerConfiguration: 3 owned collections
- CustomerDto: 3 new List properties
- CrmMappingConfig: 3 new mappings
- Frontend types: AccountRefDto, AgreementRefDto, PaymentMethodRefDto

### Non-Goals
- FK constraints to BillingAccount/Agreement/PaymentMethod tables (value objects only)
- Syncing AccountRef with actual BillingAccount lifecycle
- PaymentMethod creation/management from CRM

---

## Sub-project 5: Notification Hubs

### Scope
Add NotificationHub value object collection on Customer for TMF629-compliant notification channel configuration.

### NotificationHub Value Object
```
NotificationHub
├── Id: int (shadow PK, auto-increment)
├── HubType: enum { Email, SMS, Push, InApp, Webhook }
├── Identifier: string (200) (email address, phone number, device token, webhook URL)
├── IsOptIn: bool
├── ValidFrom: DateTime? (UTC)
├── ValidUntil: DateTime? (UTC)
├── CustomerId: Guid (FK)
```

### Table
`customer_notification_hubs` — owned collection on Customer

### Files Modified/Created
- Add `HubType` enum to `Obss.CRM.Domain.ValueObjects`
- Add `NotificationHub` value object to `Obss.CRM.Domain.ValueObjects`
- Customer: `_notificationHubs` collection + `AddNotificationHub()`, `RemoveNotificationHub()`, `SetOptIn()` methods
- CustomerConfiguration: owned collection config
- Add `NotificationHubDto`
- Add Mapster mapping
- Add commands: `AddHubCommand`, `RemoveHubCommand`, `SetHubOptInCommand`
- Add endpoints: `POST /customers/{id}/hubs`, `DELETE /customers/{id}/hubs/{hubId}`, `POST /customers/{id}/hubs/{hubId}/opt-in`
- Frontend: `NotificationHubDto` type, detail page section, edit form fields

### Non-Goals
- Webhook delivery mechanism
- Syncing with Notifications module NotificationPreference
- Push notification infrastructure

---

## Implementation Order
1. Sub-project 5 (Notification Hubs) — simplest, smallest surface area
2. Sub-project 2 (ContactMedium) — medium complexity, follows CharValue pattern exactly
3. Sub-project 4 Part A (BillingAccount) — new aggregate in Billing module
4. Sub-project 4 Part B (Agreement) — new aggregate in CRM module
5. Sub-project 4 Part C (Reference VOs on Customer) — depends on A+B

## Migration Strategy
Three separate EF migrations (one per sub-project) to the relevant databases:
- `obss_crm`: customers table additions + customer_contact_media, customer_account_refs, customer_agreement_refs, customer_payment_method_refs, customer_notification_hubs, agreements
- `obss_billing`: billing_accounts

All additive — zero existing schema changes.

## Frontend Updates
- Customer detail page: Five new sections/tabs (Contact Media, Notifications, Accounts, Agreements, Payment Methods)
- Customer edit/create form: new fields for each collection
- Each collection gets its own simple card table with add/remove inline
