# TMF629 Customer Management — Sub-project 1: Customer Entity Alignment

## Objective
Align the OBSS Customer entity with the TMF629 v5.0.1 Customer (PartyRole) model. Add PATCH support, Characteristics, CreditProfile, RelatedParty, TimePeriod, ExternalId, href, and KYC identity documents.

## Scope

### PartyRole Model
Customer becomes a PartyRole (keeps the class name `Customer` for consumer compatibility):

```
Customer (PartyRole)
 ├── EngagedParty (polymorphic FK): Individual | Organization
 ├── Description: string
 ├── StatusReason: string (attached to status transitions)
 ├── ValidFor: TimePeriod (startDateTime, endDateTime)
 ├── ExternalId: string
 ├── Href: string (REST self-link)
 ├── Characteristic[] (List<CharValue>)
 ├── CreditProfile[]
 ├── RelatedParty[]
 └── ContactMedium[] (deferred to Sub-project 2)
```

### New Entity: Individual
```
Individual : AggregateRoot<Guid>
 ├── FirstName, LastName, MiddleName
 ├── Salutation, Title
 ├── BirthDate
 ├── Nationality (country code)
 ├── Gender
 ├── KYC status (enum: NotStarted, Pending, Verified, Rejected)
 ├── KYC verified at/by
 ├── Risk rating (Low, Medium, High)
 └── IdentityDocuments[]
     └── IdentityDocument : Entity
          ├── DocumentType (NationalId, Passport, DriverLicense, ResidencePermit, Other)
          ├── DocumentNumber
          ├── IssuingAuthority, IssuingCountry
          ├── IssuedDate, ExpiryDate
          └── IsVerified
```

### New Entity: Organization
```
Organization : AggregateRoot<Guid>
 ├── TradingName
 ├── CompanyType (enum: LLC, Corporation, Partnership, SoleProprietorship, Government, NonProfit)
 ├── Industry (sector code)
 ├── RegistrationNumber
 ├── TaxNumber
 ├── CountryOfRegistration
 ├── KYC status (enum: NotStarted, Pending, Verified, Rejected)
 ├── KYC verified at/by
 └── BeneficialOwners (list of Individual/Organization refs — deferred)
```

### Value Objects
- **TimePeriod**: startDateTime, endDateTime (nullable end = ongoing)
- **CharValue**: key (string), value (string), valueType (string)
- **CreditProfile**: score (int), scoreType (string), validFor (TimePeriod), riskRating (string)
- **RelatedParty**: name (string), role (string), referredId (Guid), referredType (string)

### Existing Entity Updates
**Customer** gets:
- `IndividualId?` + `OrganizationId?` (exclusive, one non-null)
- `Description`, `StatusReason`, `ExternalId`, `Href`
- `ValidFor` (owned TimePeriod)
- `Characteristic`, `CreditProfile`, `RelatedParty` collections
- `SetStatus()` with reason parameter

### New EF Tables
- `individuals` — all Individual properties
- `individual_identity_documents` — FK to individuals, all doc properties
- `organizations` — all Organization properties
- `customer_characteristics` — customer_id, key, value, value_type
- `customer_credit_profiles` — customer_id, score, score_type, risk_rating, valid_from, valid_until
- `customer_related_parties` — customer_id, name, role, referred_id, referred_type
- Check constraint: exactly one of individual_id/organization_id is set on Customer

### New/Modified API Endpoints
| Method | Path | Handler |
|--------|------|---------|
| PATCH | `/crm/customers/{id}` | `PartialUpdateCustomerCommand` |
| POST | `/crm/customers/{id}/characteristics` | `AddCharacteristicCommand` |
| DELETE | `/crm/customers/{id}/characteristics/{key}` | `RemoveCharacteristicCommand` |
| POST | `/crm/customers/{id}/credit-profiles` | `AddCreditProfileCommand` |
| GET | `/crm/customers/{id}/credit-profiles` | `GetCreditProfilesQuery` |
| POST | `/crm/customers/{id}/related-parties` | `AddRelatedPartyCommand` |
| DELETE | `/crm/customers/{id}/related-parties/{referredId}` | `RemoveRelatedPartyCommand` |
| POST | `/crm/individuals` | `CreateIndividualCommand` |
| GET | `/crm/individuals/{id}` | `GetIndividualByIdQuery` |
| PUT | `/crm/individuals/{id}` | `UpdateIndividualCommand` |
| POST | `/crm/organizations` | `CreateOrganizationCommand` |
| GET | `/crm/organizations/{id}` | `GetOrganizationByIdQuery` |
| PUT | `/crm/organizations/{id}` | `UpdateOrganizationCommand` |
| POST | `/crm/customers/{id}/kyc-verify` | `VerifyCustomerKycCommand` |
| POST | `/crm/individuals/{individualId}/documents` | `AddIdentityDocumentCommand` |
| DELETE | `/crm/individuals/{individualId}/documents/{docId}` | `RemoveIdentityDocumentCommand` |

Existing POST/PUT /customers accept `individual` and `organization` nested objects for creation in one call.

### DTOs
New DTOs: `IndividualDto`, `OrganizationDto`, `IdentityDocumentDto`, `CharValueDto`, `CreditProfileDto`, `RelatedPartyDto`

CustomerDto gains: `description`, `statusReason`, `validFrom`, `validUntil`, `externalId`, `href`, `individual`, `organization`, `characteristics`, `creditProfiles`, `relatedParties`

### Frontend Updates
- New types: IndividualDto, OrganizationDto, IdentityDocumentDto, CharValueDto, CreditProfileDto, RelatedPartyDto
- CustomerDetailPage: new sections for Individual/Organization info, Characteristic table, Credit Profile history, Related Parties
- Create/Edit customer forms: description field, validFor date pickers, option to create Individual or Organization inline
- New pages: `/customers/[id]/individual` and `/customers/[id]/organization` for managing engaged party details
- Edit page uses PATCH instead of PUT

### Implementation Order
1. TimePeriod value object
2. Individual + IdentityDocument entities
3. Organization entity
4. CharValue, CreditProfile, RelatedParty value objects/entities
5. Customer entity updates (EngagedParty FK, new fields, SetStatus with reason)
6. EF Core configurations (all new tables + Customer columns)
7. New DTOs + update CustomerDto
8. Mapster mapping config
9. Application Commands: PartialUpdateCustomer, AddCharacteristic, RemoveCharacteristic, AddCreditProfile, AddRelatedParty, RemoveRelatedParty, CreateIndividual, UpdateIndividual, CreateOrganization, UpdateOrganization, VerifyCustomerKyc, AddIdentityDocument, RemoveIdentityDocument
10. Application Queries: GetCreditProfiles, GetIndividualById, GetOrganizationById
11. API Endpoints (all new + PATCH)
12. Frontend types update
13. Frontend detail page updates (new sections)
14. Frontend create/edit form updates
15. Frontend individual/organization sub-pages
16. Build verification

### Non-Goals (deferred)
- ContactMedium enrichment (Sub-project 2)
- AccountRef, AgreementRef, PaymentMethodRef (Sub-project 4)
- Notification Hubs (Sub-project 5)
- CRM Lookup endpoints for TMF enums
- Attribute selection (`?fields=`)
- TMF-standard pagination (offset/limit)
