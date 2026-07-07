# WP-014: Fix CRM Dual Aggregate Root

**Date:** 2026-07-07
**Roadmap Ref:** Wave 1, WP-014
**Goal:** Demote Individual and Organization from `AggregateRoot<Guid>` to `Entity<Guid>` and route all access through the Customer aggregate.

---

## 1. Domain Changes

### Individual
- **Current:** `Individual : AggregateRoot<Guid>` with 109 lines, 6 properties, IdentityDocument children
- **New:** `Individual : Entity<Guid>`
- Keep all properties, methods, IdentityDocument child collection
- Remove the parameterless `private Individual() { }` EF constructor (use the existing one)

### Organization
- **Current:** `Organization : AggregateRoot<Guid>` with 75 lines, 8 properties
- **New:** `Organization : Entity<Guid>`
- Keep all properties, methods

### Customer
- Add `CreateIndividual(...)` static factory — creates Individual and assigns it to the Customer
- Add `CreateOrganization(...)` static factory — creates Organization and assigns it to the Customer
- Add `UpdateIndividual(...)` method — updates Individual properties
- Add `UpdateOrganization(...)` method — updates Organization properties
- Add `AddIdentityDocument(...)` method — adds document to Individual
- Add `RemoveIdentityDocument(...)` method — removes document from Individual

---

## 2. Handler Changes

### Refactor 6 handlers:

| Current Handler | New Approach |
|----------------|-------------|
| `CreateIndividualCommandHandler` | Inject `ICustomerRepository`, create Customer with Individual |
| `UpdateIndividualCommandHandler` | Load Customer via repo, call `customer.UpdateIndividual(...)` |
| `GetIndividualByIdQueryHandler` | Load Customer via repo, return its Individual |
| `AddIdentityDocumentCommandHandler` | Load Customer via repo, call `customer.AddIdentityDocument(...)` |
| `RemoveIdentityDocumentCommandHandler` | Load Customer via repo, call `customer.RemoveIdentityDocument(...)` |
| `CreateOrganizationCommandHandler` | Inject `ICustomerRepository`, create Customer with Organization |
| `UpdateOrganizationCommandHandler` | Load Customer via repo, call `customer.UpdateOrganization(...)` |
| `GetOrganizationByIdQueryHandler` | Load Customer via repo, return its Organization |

---

## 3. API Endpoints

Keep existing endpoints but internally route through Customer:

- `POST /individuals` → CreateIndividualCommand → Customer.CreateIndividual()
- `GET /individuals/{id}` → GetIndividualByIdQuery → load Customer, return Individual
- `PUT /individuals/{id}` → UpdateIndividualCommand → Customer.UpdateIndividual()
- `POST /organizations` → CreateOrganizationCommand → Customer.CreateOrganization()
- `GET /organizations/{id}` → GetOrganizationByIdQuery → load Customer, return Organization
- `PUT /organizations/{id}` → UpdateOrganizationCommand → Customer.UpdateOrganization()

---

## 4. EF Core Configuration

No schema changes needed. Individual/Organization remain in `individuals`/`organizations` tables.

Update `CustomerConfiguration`:
- `HasOne(c => c.Individual).WithMany().HasForeignKey(c => c.IndividualId)` stays
- `HasOne(c => c.Organization).WithMany().HasForeignKey(c => c.OrganizationId)` stays
- No changes to `IndividualConfiguration` or `OrganizationConfiguration`

---

## 5. Out of Scope

- CustomerSegment and Agreement: remain independent aggregate roots
- Customer DTO mapping: no changes needed
- Frontend: no changes
