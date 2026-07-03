# TMF620 Product Catalog Alignment — Comprehensive Design

## Objective
Extend the OBSS ProductCatalog module with ProductSpecification (full entity), ProductOfferingTerm, BundledProductOffering, and pricing enhancements to complete TMF620 v5.0.1 alignment.

## Architecture
Four additive sub-projects executed sequentially. All changes target `obss_catalog` database. All new entities follow the same patterns as existing module: AggregateRoot/Entity base classes, ITenantEntity, string-based enum conversions, ValueGeneratedNever for IDs, Mapster DTOs, MediatR commands/queries, FluentValidation, RESTful endpoints under `/api/v{version}/catalog`.

## Tech Stack
Same as existing module: .NET 9, EF Core/Npgsql, MediatR, FluentValidation, Mapster, Next.js 16, React Query, zod.

## Principle
All changes are additive and non-breaking. Existing entities, fields, DTOs, and endpoints are preserved. New fields are nullable/optional.

---

## Sub-project 1: ProductSpecification Entity

### Scope
Promote ProductSpecification from a value object (ad-hoc Name/Value/IsRequired pairs on Product) to a full aggregate root with characteristics, characteristic values, relationships, and product number/SKU.

### New Entities

#### ProductSpecification (AggregateRoot<Guid>, ITenantEntity)
```
ProductSpecification
├── Id: Guid
├── TenantId: string
├── Name: string
├── Description: string?
├── Brand: string?
├── Version: string?
├── ProductNumber: string? (SKU — unique per tenant)
├── LifecycleStatus: LifecycleStatus (Draft/Active/Retired/Discontinued)
├── ValidFrom: DateTime? (UTC)
├── ValidTo: DateTime? (UTC)
├── CreatedAt: DateTime (UTC)
├── UpdatedAt: DateTime (UTC)
├── Characteristics: IReadOnlyCollection<ProductSpecificationCharacteristic>
└── Relationships: IReadOnlyCollection<ProductSpecificationRelationship>
```

**Lifecycle methods:** `Create()`, `Activate()`, `Retire()`, `Discontinue()`, `UpdateDetails()`
**Domain events:** `ProductSpecificationCreatedDomainEvent`

#### ProductSpecificationCharacteristic (Entity<Guid>)
```
ProductSpecificationCharacteristic
├── Id: Guid
├── ProductSpecificationId: Guid (FK)
├── Name: string
├── Description: string?
├── ValueType: string — "string", "number", "boolean", "date", "quantity"
├── Configurable: bool (default: true — can be overridden at product level)
├── MinValue: decimal?
├── MaxValue: decimal?
├── Regex: string?
├── SortOrder: int
├── MaxCardinality: int? (max number of values that can be selected)
├── IsRequired: bool
├── Values: IReadOnlyCollection<ProductSpecificationCharacteristicValue>
```

#### ProductSpecificationCharacteristicValue (Entity<Guid>)
```
ProductSpecificationCharacteristicValue
├── Id: Guid
├── CharacteristicId: Guid (FK)
├── Value: string
├── UnitOfMeasure: string?
├── IsDefault: bool
├── ValueFrom: decimal?
├── ValueTo: decimal?
├── RangeInterval: string? — e.g. "step 5"
├── ValidFrom: DateTime? (UTC)
├── ValidTo: DateTime? (UTC)
```

#### ProductSpecificationRelationship (Entity<Guid>)
```
ProductSpecificationRelationship
├── Id: Guid
├── ProductSpecificationId: Guid (FK)
├── TargetSpecificationId: Guid (FK — references another ProductSpecification)
├── RelationshipType: SpecificationRelationshipType (enum)
├── Role: string? — e.g. "mandatory", "optional"
├── ValidFrom: DateTime? (UTC)
├── ValidTo: DateTime? (UTC)
```

**Enum: SpecificationRelationshipType**
- `Dependency = 1` — this spec depends on target
- `Substitution = 2` — can substitute target
- `Exclusion = 3` — cannot coexist with target
- `Optional = 4` — target is optional companion

### Changes to Product Entity

Additive (non-breaking):
- `ProductNumber: string?` — SKU, unique per tenant
- `ProductSpecificationId: Guid?` (FK → ProductSpecification)
- `ProductSpecification: ProductSpecification?` — navigation property

The existing `ProductSpecification` value object (Name/Value/IsRequired) is preserved unchanged — it represents ad-hoc product attributes distinct from the formal specification.

### EF Configurations

**ProductSpecificationConfiguration:**
- Table: `product_specifications`
- Indexes: TenantId, Name, LifecycleStatus, (TenantId, ProductNumber) unique with filter
- OwnsMany: Characteristics → `product_specification_characteristics`
- OwnsMany: Relationships → `product_specification_relationships`

**ProductSpecificationCharacteristicConfiguration:**
- Table: `product_specification_characteristics`
- OwnsMany: Values → `product_specification_characteristic_values`

**ProductSpecificationRelationshipConfiguration:**
- Table: `product_specification_relationships`

**Product update:**
- Add navigation to ProductSpecification in ProductConfiguration
- Add ProductNumber column

### DTOs

```
ProductSpecificationDto
├── Id, Name, Description, Brand, Version, ProductNumber
├── LifecycleStatus, ValidFrom, ValidTo
├── Characteristics: ProductSpecificationCharacteristicDto[]
└── Relationships: ProductSpecificationRelationshipDto[]

ProductSpecificationCharacteristicDto
├── Id, Name, Description, ValueType, Configurable
├── MinValue, MaxValue, Regex, SortOrder, MaxCardinality, IsRequired
└── Values: ProductSpecificationCharacteristicValueDto[]

ProductSpecificationCharacteristicValueDto
├── Id, Value, UnitOfMeasure, IsDefault
├── ValueFrom, ValueTo, RangeInterval
├── ValidFrom, ValidTo

ProductSpecificationRelationshipDto
├── Id, RelationshipType, TargetSpecificationId, Role
├── ValidFrom, ValidTo
```

ProductDto gets new optional fields: `ProductNumber`, `ProductSpecificationId`, `ProductSpecification` (nested, included when expanded).

### Endpoints

```
GET    /api/v{version}/catalog/product-specifications                       — list (paginated, filterable)
GET    /api/v{version}/catalog/product-specifications/{id}                  — by id
POST   /api/v{version}/catalog/product-specifications                       — create (with characteristics/values)
PUT    /api/v{version}/catalog/product-specifications/{id}                  — full update
PATCH  /api/v{version}/catalog/product-specifications/{id}                  — partial update
DELETE /api/v{version}/catalog/product-specifications/{id}                  — soft delete

GET    /api/v{version}/catalog/product-specifications/{id}/characteristics                 — list characteristics
POST   /api/v{version}/catalog/product-specifications/{id}/characteristics                 — add characteristic
PUT    /api/v{version}/catalog/product-specifications/{specId}/characteristics/{charId}     — update characteristic
DELETE /api/v{version}/catalog/product-specifications/{specId}/characteristics/{charId}     — remove characteristic

GET    /api/v{version}/catalog/product-specifications/{specId}/characteristics/{charId}/values          — list values
POST   /api/v{version}/catalog/product-specifications/{specId}/characteristics/{charId}/values          — add value
PUT    /api/v{version}/catalog/product-specifications/{specId}/characteristics/{charId}/values/{valueId} — update value
DELETE /api/v{version}/catalog/product-specifications/{specId}/characteristics/{charId}/values/{valueId} — remove value

GET    /api/v{version}/catalog/product-specifications/{id}/relationships          — list relationships
POST   /api/v{version}/catalog/product-specifications/{id}/relationships          — add relationship
DELETE /api/v{version}/catalog/product-specifications/{id}/relationships/{relId}   — remove relationship
```

### Commands & Queries

Commands:
- `CreateProductSpecificationCommand` (with nested characteristics/values)
- `UpdateProductSpecificationCommand`
- `PatchProductSpecificationCommand`
- `DeleteProductSpecificationCommand`
- `AddCharacteristicCommand`
- `UpdateCharacteristicCommand`
- `RemoveCharacteristicCommand`
- `AddCharacteristicValueCommand`
- `UpdateCharacteristicValueCommand`
- `RemoveCharacteristicValueCommand`
- `AddSpecificationRelationshipCommand`
- `RemoveSpecificationRelationshipCommand`

Queries:
- `GetProductSpecificationsQuery` (paginated, filterable by Name/LifecycleStatus/Brand)
- `GetProductSpecificationByIdQuery` (with characteristics/values/relationships)

### Frontend

New types in `dto.ts`: (following existing naming conventions)
- `ProductSpecificationDto`
- `ProductSpecificationCharacteristicDto`
- `ProductSpecificationCharacteristicValueDto`
- `ProductSpecificationRelationshipDto`

Exported from `index.ts`.

---

## Sub-project 2: ProductOfferingTerm

### Scope
Add formal contract/renewal/cancellation term modeling to Offer as a child entity collection.

### New Entity

#### ProductOfferingTerm (Entity<Guid>)
```
ProductOfferingTerm
├── Id: Guid
├── OfferId: Guid (FK — with cascade)
├── Name: string — e.g. "Minimum Contract Term", "Renewal Term"
├── Description: string?
├── Duration: int
├── DurationUnit: DurationUnit (Days/Months/Years)
├── TermType: TermType (MinimumContract/Renewal/Cancellation)
├── ValidFrom: DateTime? (UTC)
├── ValidTo: DateTime? (UTC)
```

**Enums:**
- `TermType`: `MinimumContract = 1, Renewal = 2, Cancellation = 3`
- `DurationUnit`: `Days = 1, Months = 2, Years = 3`

### Changes to Offer Entity
Additive:
- `_terms: List<ProductOfferingTerm>` backing field
- `Terms: IReadOnlyCollection<ProductOfferingTerm>` read-only property

Existing `Offer.IsContract`, `Offer.ContractDurationMonths` fields are retained for backward compatibility.

### EF Configuration
- Table: `product_offering_terms`
- Indexes: OfferId, TermType

### DTOs
```
ProductOfferingTermDto
├── Id, Name, Description, Duration, DurationUnit, TermType
├── ValidFrom, ValidTo
```

`OfferDto` gets new optional field: `Terms: ProductOfferingTermDto[]`

### Endpoints
```
GET    /api/v{version}/catalog/offers/{offerId}/terms          — list terms for offer
POST   /api/v{version}/catalog/offers/{offerId}/terms          — add term
PUT    /api/v{version}/catalog/offers/{offerId}/terms/{termId}  — update term
DELETE /api/v{version}/catalog/offers/{offerId}/terms/{termId}  — remove term
```

### Commands & Queries
- `AddProductOfferingTermCommand`
- `UpdateProductOfferingTermCommand`
- `RemoveProductOfferingTermCommand`
- `GetProductOfferingTermsQuery`

### Frontend
- `ProductOfferingTermDto` in `dto.ts` and `index.ts`

---

## Sub-project 3: BundledProductOffering

### Scope
Add bundle composition support to Offer — allowing an offer to contain other offers.

### New Entity

#### BundledProductOffering (Entity<Guid>)
```
BundledProductOffering
├── Id: Guid
├── OfferId: Guid (FK — the parent bundle offer)
├── BundledOfferId: Guid (FK — the offer being bundled)
├── BundledOffer: Offer? (navigation)
├── Name: string? (optional override)
├── Quantity: int (default: 1)
├── ReferralType: string? (e.g. "Standalone", "Optional", "Mandatory")
```

No new enums — `ReferralType` is a free-text string per TMF620.

### Changes to Offer Entity
Additive:
- `_bundledOfferings: List<BundledProductOffering>` backing field
- `BundledOfferings: IReadOnlyCollection<BundledProductOffering>` read-only property
- `OfferType.Bundled` — set automatically in `AddBundledOffering()` lifecycle method

### EF Configuration
- Table: `bundled_product_offerings`
- Indexes: OfferId, BundledOfferId
- Unique constraint: (OfferId, BundledOfferId)
- Relationship: BundledOfferId → Offer.Id (no cascade to avoid cycles)

### DTOs
```
BundledProductOfferingDto
├── Id, Name, Quantity, ReferralType
└── BundledOffer: OfferDto? (nested, included when expanded)
```

`OfferDto` gets new optional field: `BundledOfferings: BundledProductOfferingDto[]`

### Endpoints
```
GET    /api/v{version}/catalog/offers/{offerId}/bundled-offerings          — list bundled offerings
POST   /api/v{version}/catalog/offers/{offerId}/bundled-offerings          — add bundled offering
PUT    /api/v{version}/catalog/offers/{offerId}/bundled-offerings/{id}     — update
DELETE /api/v{version}/catalog/offers/{offerId}/bundled-offerings/{id}     — remove
```

### Commands & Queries
- `AddBundledProductOfferingCommand`
- `UpdateBundledProductOfferingCommand`
- `RemoveBundledProductOfferingCommand`
- `GetBundledProductOfferingsQuery`

### Frontend
- `BundledProductOfferingDto` in `dto.ts` and `index.ts`

---

## Sub-project 4: Pricing Enhancements

### Scope
Add Name, Description, PriceType, RecurringChargePeriod to OfferPricing. Add PriceRange child entity for explicit tiered pricing.

### Changes to OfferPricing Entity
Additive (existing fields untouched):
- `Name: string?`
- `Description: string?`
- `PriceType: PriceType?` (enum: OneTime, Recurring, Usage) — when null, behavior is backward-compatible
- `RecurringChargePeriod: BillingPeriod?`

Existing `OneTimePrice`, `RecurringPrice`, `UsagePrice`, `PricingType`, `Currency`, `UnitOfMeasure`, `MinQuantity`, `MaxQuantity`, `IsActive` fields are preserved.

### New Entity

#### PriceRange (Entity<Guid>)
```
PriceRange
├── Id: Guid
├── OfferPricingId: Guid (FK — with cascade)
├── MinQuantity: int
├── MaxQuantity: int? (null = unlimited)
├── Price: decimal
├── IsActive: bool
```

### EF Configuration
- Table: `price_ranges`
- Indexes: OfferPricingId

### DTOs
```
PriceRangeDto
├── Id, MinQuantity, MaxQuantity, Price, IsActive
```

`OfferPricingDto` gets new optional fields: `Name`, `Description`, `PriceType`, `RecurringChargePeriod`, `PriceRanges: PriceRangeDto[]`

### Endpoints
```
POST   /api/v{version}/catalog/offers/{offerId}/pricing/{pricingId}/price-ranges          — add range
PUT    /api/v{version}/catalog/offers/{offerId}/pricing/{pricingId}/price-ranges/{rangeId} — update range
DELETE /api/v{version}/catalog/offers/{offerId}/pricing/{pricingId}/price-ranges/{rangeId} — remove range
GET    /api/v{version}/catalog/offers/{offerId}/pricing/{pricingId}/price-ranges           — list ranges
```

### Commands & Queries
- `AddPriceRangeCommand`
- `UpdatePriceRangeCommand`
- `RemovePriceRangeCommand`
- `GetPriceRangesQuery`

### Frontend
- `PriceRangeDto` in `dto.ts` and `index.ts`
- `OfferPricingDto` gets new optional fields

---

## Database Migration Strategy

All four sub-projects target `obss_catalog`. Separate migrations:

| Migration | Tables Added | Columns Added |
|-----------|-------------|---------------|
| `Tmf620ProductSpecification` | `product_specifications`, `product_specification_characteristics`, `product_specification_characteristic_values`, `product_specification_relationships` | `products.ProductNumber`, `products.ProductSpecificationId` |
| `Tmf620ProductOfferingTerm` | `product_offering_terms` | — |
| `Tmf620BundledProductOffering` | `bundled_product_offerings` | — |
| `Tmf620PricingEnhancements` | `price_ranges` | `offer_pricings.Name`, `offer_pricings.Description`, `offer_pricings.PriceType`, `offer_pricings.RecurringChargePeriod` |

## Implementation Order
1. Sub-project 1 (ProductSpecification) — foundation for spec-driven catalog
2. Sub-project 2 (ProductOfferingTerm) — independent from SP1
3. Sub-project 3 (BundledProductOffering) — independent from SP1/SP2
4. Sub-project 4 (Pricing Enhancements) — can run in parallel with SP2/SP3

## Non-Goals
- Bulk import/export (e.g., CSV/Excel catalog loading)
- ProductSpecification → Product catalogs/comparisons
- Price events or price change history tracking
- Attachment/RelatedParty on ProductSpecification (TMF optional attributes)
- Frontend pages for new entities (types only — UI deferred)
