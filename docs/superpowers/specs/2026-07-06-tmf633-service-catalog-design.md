# TMF633 Service Catalog — Design Specification

**Version:** 1.0
**Date:** 2026-07-06
**Status:** Approved for Implementation
**TMF API:** TMF633 — Service Catalog Management

---

## 1. Overview

Standalone module (`Obss.ServiceCatalog`) implementing TMF633 Service Catalog Management API.
Follows existing 4-layer Clean Architecture pattern (Api/Application/Domain/Infrastructure)
with DDD, CQRS, and EF Core.

**Module path:** `src/Modules/ServiceCatalog/`

**Integration targets:** ProductCatalog (`ProductSpecification.ServiceSpecificationId`),
ServiceInventory (`Service.ServiceSpecificationId`), Provisioning (`ProvisioningTemplate.ServiceSpecificationId`)

---

## 2. Domain Entities

### 2.1 ServiceCategory (AggregateRoot<Guid>, ITenantEntity)

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| TenantId | string | Max 100 |
| Name | string | Max 200 |
| Description | string? | Max 2000 |
| ParentCategoryId | Guid? | Self-referencing hierarchy |
| LifecycleStatus | LifecycleStatus | enum: Draft, Active, Retired |
| Version | int | Default 1 |
| ValidFrom | DateTime? | |
| ValidTo | DateTime? | |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |
| IsRoot | bool | Computed: !ParentCategoryId.HasValue |

**Behaviors:** `Create()`, `Activate()`, `Retire()`, `UpdateDetails()`, `SetValidityPeriod()`,
`AddCandidate()`, `RemoveCandidate()`

**Collections:** `ServiceCandidates` (many-to-many via join table)

### 2.2 ServiceCandidate (AggregateRoot<Guid>, ITenantEntity)

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| TenantId | string | Max 100 |
| Name | string | Max 200 |
| Description | string? | Max 2000 |
| LifecycleStatus | LifecycleStatus | Draft, Active, Retired |
| Version | int | Default 1 |
| ValidFrom | DateTime? | |
| ValidTo | DateTime? | |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |
| ServiceSpecificationId | Guid? | FK to ServiceSpecification |
| BaseCandidateId | Guid? | For versioning/original source |
| FeatureSpecification | string? | JSONB — additional features |
| Categories | IReadOnlyCollection<ServiceCategory> | Many-to-many |

**Behaviors:** `Create()`, `Activate()`, `Retire()`, `UpdateDetails()`, `AssignSpecification()`

### 2.3 ServiceSpecification (AggregateRoot<Guid>, ITenantEntity)

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| TenantId | string | Max 100 |
| Name | string | Max 200 |
| Description | string? | Max 2000 |
| Brand | string? | Max 200 |
| Version | string? | Max 100 |
| LifecycleStatus | LifecycleStatus | Draft, Active, Retired |
| IsBundle | bool | |
| ValidFrom | DateTime? | |
| ValidTo | DateTime? | |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

**Behaviors:** `Create()`, `Activate()`, `Retire()`, `UpdateDetails()`, `SetValidityPeriod()`,
`AddCharacteristic()`, `RemoveCharacteristic()`, `AddRelationship()`, `RemoveRelationship()`

**Collections:** `Characteristics` (auto-included), `Relationships` (auto-included)

### 2.4 ServiceSpecCharacteristic (Entity<Guid>)

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| ServiceSpecificationId | Guid | FK |
| Name | string | Max 200 |
| Description | string? | Max 2000 |
| ValueType | string | Max 50 (string, integer, decimal, date, boolean) |
| Configurable | bool | Default true |
| MinValue | decimal? | (18,4) |
| MaxValue | decimal? | (18,4) |
| Regex | string? | Max 500 |
| SortOrder | int | |
| MaxCardinality | int? | |
| IsRequired | bool | |

**Behaviors:** `UpdateDetails()`, `AddValue()`, `RemoveValue()`

**Collections:** `Values` (auto-included)

### 2.5 ServiceSpecCharValue (Entity<Guid>)

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| CharacteristicId | Guid | FK |
| Value | string | Max 1000 |
| UnitOfMeasure | string? | Max 50 |
| IsDefault | bool | |
| ValueFrom | string? | Max 50 |
| ValueTo | string? | Max 50 |
| RangeInterval | string? | Max 50 |
| ValidFrom | DateTime? | |
| ValidTo | DateTime? | |

**Behavior:** `Update()`

### 2.6 ServiceSpecRelationship (Entity<Guid>)

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| ServiceSpecificationId | Guid | FK |
| TargetSpecificationId | Guid | FK |
| RelationshipType | RelationshipType | enum: ReliesOn, DependsOn, Aggregates, IsConnectedTo |
| Role | string? | Max 100 |
| ValidFrom | DateTime? | |
| ValidTo | DateTime? | |

### 2.7 Value Objects & Enums

| Enum | Values |
|------|--------|
| LifecycleStatus | Draft, Active, Retired |
| RelationshipType | ReliesOn, DependsOn, Aggregates, IsConnectedTo |

---

## 3. Application Layer

### 3.1 DTOs

- `ServiceCategoryDto` — all Category fields
- `ServiceCandidateDto` — all Candidate fields + Categories list + ServiceSpecificationName
- `ServiceSpecificationDto` — all Spec fields + Characteristics + Relationships
- `ServiceSpecCharacteristicDto` — all Characteristic fields + Values
- `ServiceSpecCharValueDto` — all Value fields
- `ServiceSpecRelationshipDto` — all Relationship fields
- `ServiceSummaryDto` — Id, Name, LifecycleStatus, Version, ValidFrom, ValidTo (for list views)

### 3.2 Commands (13 handlers)

`CreateServiceCategory`, `UpdateServiceCategory`, `DeleteServiceCategory`,
`CreateServiceCandidate`, `UpdateServiceCandidate`, `DeleteServiceCandidate`,
`CreateServiceSpecification`, `UpdateServiceSpecification`, `DeleteServiceSpecification`,
`AddCharacteristic`, `RemoveCharacteristic`, `AddCharacteristicValue`, `RemoveCharacteristicValue`,
`AddSpecRelationship`, `RemoveSpecRelationship`

### 3.3 Queries (9 handlers)

`GetServiceCategories`, `GetServiceCategoryById`, `GetServiceCandidates`, `GetServiceCandidateById`,
`GetServiceSpecifications`, `GetServiceSpecificationById`, `GetCharacteristics`, `GetCharacteristicValues`,
`GetSpecRelationships`

### 3.4 Validators

All commands with FluentValidation covering required fields, max lengths, lifecycle state transitions.

### 3.5 Mappings (Mapster)

`ServiceCatalogMappingConfig.Configure()` — entity-to-DTO mappings.

---

## 4. API Endpoints

**Route group:** `/api/v{version}/service-catalog`

| Method | Path | Handler |
|--------|------|---------|
| POST | `/service-categories` | CreateServiceCategoryCommand |
| GET | `/service-categories` | GetServiceCategoriesQuery |
| GET | `/service-categories/{id}` | GetServiceCategoryByIdQuery |
| PATCH | `/service-categories/{id}` | UpdateServiceCategoryCommand |
| DELETE | `/service-categories/{id}` | DeleteServiceCategoryCommand |
| POST | `/service-candidates` | CreateServiceCandidateCommand |
| GET | `/service-candidates` | GetServiceCandidatesQuery |
| GET | `/service-candidates/{id}` | GetServiceCandidateByIdQuery |
| PATCH | `/service-candidates/{id}` | UpdateServiceCandidateCommand |
| DELETE | `/service-candidates/{id}` | DeleteServiceCandidateCommand |
| POST | `/service-specifications` | CreateServiceSpecificationCommand |
| GET | `/service-specifications` | GetServiceSpecificationsQuery |
| GET | `/service-specifications/{id}` | GetServiceSpecificationByIdQuery |
| PATCH | `/service-specifications/{id}` | UpdateServiceSpecificationCommand |
| DELETE | `/service-specifications/{id}` | DeleteServiceSpecificationCommand |
| POST | `/service-specifications/{id}/characteristics` | AddCharacteristicCommand |
| GET | `/service-specifications/{id}/characteristics` | GetCharacteristicsQuery |
| PUT | `/service-specifications/{id}/characteristics/{charId}` | UpdateCharacteristicCommand |
| DELETE | `/service-specifications/{id}/characteristics/{charId}` | RemoveCharacteristicCommand |
| POST | `/service-specifications/{id}/characteristics/{charId}/values` | AddCharacteristicValueCommand |
| PUT | `/service-specifications/{id}/characteristics/{charId}/values/{valId}` | UpdateCharacteristicValueCommand |
| DELETE | `/service-specifications/{id}/characteristics/{charId}/values/{valId}` | RemoveCharacteristicValueCommand |
| POST | `/service-specifications/{id}/relationships` | AddSpecRelationshipCommand |
| DELETE | `/service-specifications/{id}/relationships/{relId}` | RemoveSpecRelationshipCommand |

Response pattern: Standard with `X-Total-Count`, `X-Result-Count` headers, pagination support.

---

## 5. Infrastructure Layer

### 5.1 Entity Configurations (EF Core Fluent API)

Snake_case column names, `ValueGeneratedNever()` for Guid PKs.
Enums stored as strings. Indexes on tenant_id, lifecycle_status, name, parent_category_id.

### 5.2 DbContext

`ServiceCatalogDbContext` extending `EfDbContext` with DbSets for all 6 entities.

### 5.3 Repositories

`IServiceCategoryRepository`, `IServiceCandidateRepository`, `IServiceSpecificationRepository`
following existing `IProductRepository`, `IOfferRepository` patterns.

---

## 6. Database Schema

```
service_catalog schema (PostgreSQL):
├── service_categories
│   ├── id (uuid PK)
│   ├── tenant_id (varchar 100)
│   ├── name (varchar 200)
│   ├── description (varchar 2000, nullable)
│   ├── parent_category_id (uuid, nullable, FK self)
│   ├── lifecycle_status (varchar 20)
│   ├── version (int)
│   ├── valid_from (timestamptz, nullable)
│   ├── valid_to (timestamptz, nullable)
│   ├── created_at (timestamptz)
│   └── updated_at (timestamptz)
├── service_candidates
│   ├── id (uuid PK)
│   ├── tenant_id (varchar 100)
│   ├── name (varchar 200)
│   ├── description (varchar 2000, nullable)
│   ├── lifecycle_status (varchar 20)
│   ├── version (int)
│   ├── valid_from (timestamptz, nullable)
│   ├── valid_to (timestamptz, nullable)
│   ├── service_specification_id (uuid, nullable, FK)
│   ├── base_candidate_id (uuid, nullable, FK self)
│   ├── feature_specification (jsonb, nullable)
│   ├── created_at (timestamptz)
│   └── updated_at (timestamptz)
├── category_candidates
│   ├── category_id (uuid, FK)
│   └── candidate_id (uuid, FK) — composite PK
├── service_specifications
│   ├── id (uuid PK)
│   ├── tenant_id (varchar 100)
│   ├── name (varchar 200)
│   ├── description (varchar 2000, nullable)
│   ├── brand (varchar 200, nullable)
│   ├── version (varchar 100, nullable)
│   ├── lifecycle_status (varchar 20)
│   ├── is_bundle (boolean)
│   ├── valid_from (timestamptz, nullable)
│   ├── valid_to (timestamptz, nullable)
│   ├── created_at (timestamptz)
│   └── updated_at (timestamptz)
├── service_spec_characteristics
│   ├── id (uuid PK)
│   ├── service_specification_id (uuid, FK)
│   ├── name (varchar 200)
│   ├── description (varchar 2000, nullable)
│   ├── value_type (varchar 50)
│   ├── configurable (boolean)
│   ├── min_value (decimal 18,4, nullable)
│   ├── max_value (decimal 18,4, nullable)
│   ├── regex (varchar 500, nullable)
│   ├── sort_order (int)
│   ├── max_cardinality (int, nullable)
│   ├── is_required (boolean)
├── service_spec_characteristic_values
│   ├── id (uuid PK)
│   ├── characteristic_id (uuid, FK)
│   ├── value (varchar 1000)
│   ├── unit_of_measure (varchar 50, nullable)
│   ├── is_default (boolean)
│   ├── value_from (varchar 50, nullable)
│   ├── value_to (varchar 50, nullable)
│   ├── range_interval (varchar 50, nullable)
│   ├── valid_from (timestamptz, nullable)
│   ├── valid_to (timestamptz, nullable)
├── service_spec_relationships
│   ├── id (uuid PK)
│   ├── service_specification_id (uuid, FK)
│   ├── target_specification_id (uuid, FK)
│   ├── relationship_type (varchar 30)
│   ├── role (varchar 100, nullable)
│   ├── valid_from (timestamptz, nullable)
│   └── valid_to (timestamptz, nullable)
```

---

## 7. Frontend

**Pages** under `frontend/src/app/service-catalog/`:
- `page.tsx` — Dashboard with 3 tabs (Categories, Candidates, Specifications)
- `categories/page.tsx` — Tree hierarchy view + DataTable
- `categories/[id]/page.tsx` — Detail with service candidates list
- `categories/new/page.tsx` — Create form (name, description, parent)
- `candidates/page.tsx` — DataTable with status/type filters
- `candidates/[id]/page.tsx` — Detail with specification reference
- `candidates/new/page.tsx` — Create form with category picker
- `specifications/page.tsx` — DataTable
- `specifications/[id]/page.tsx` — Detail with characteristics tab, relationships tab
- `specifications/[id]/edit/page.tsx` — Edit form
- `specifications/new/page.tsx` — Create form

**React Query hooks** (following `useProducts`, `useOffers` pattern):
- `useServiceCategories`, `useServiceCategory(id)`
- `useCreateServiceCategory`, `useUpdateServiceCategory`, `useDeleteServiceCategory`
- `useServiceCandidates`, `useServiceCandidate(id)`
- `useCreateServiceCandidate`, `useUpdateServiceCandidate`, `useDeleteServiceCandidate`
- `useServiceSpecifications`, `useServiceSpecification(id)`
- `useCreateServiceSpecification`, `useUpdateServiceSpecification`
- `useServiceCharacteristics(specId)`, `useCharacteristicValues(charId)`

---

## 8. Integration Plan

### 8.1 ProductCatalog

Add optional `ServiceSpecificationId` (Guid?) to `ProductSpecification` entity.
Add `GetServiceSpecificationById` query integration (via HTTP or direct MediatR).

### 8.2 ServiceInventory

Replace free-text `ServiceType` enum with `ServiceSpecificationId` (Guid?).
Service lifecycle events can reference the TMF633 specification.

### 8.3 Provisioning

`ProvisioningTemplate.ServiceType` becomes `ServiceSpecificationId` (Guid?).
Provisioning jobs reference the service specification for appropriate templates.

---

## 9. Module Registration

Standard pattern following `CatalogModuleRegistration`, `CrmModuleRegistration`:

```csharp
services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
services.AddScoped<IServiceCandidateRepository, ServiceCandidateRepository>();
services.AddScoped<IServiceSpecificationRepository, ServiceSpecificationRepository>();
ServiceCatalogMappingConfig.Configure();
```

Endpoints mapped to `/api/v{version}/service-catalog`.

---

## 10. Tests

Following existing module test patterns under `tests/Modules/ServiceCatalog.Tests/`:
- `CommandHandlerTests.cs` — all 13 command handlers
- `RepositoryTests.cs` — all 3 repositories
- `ServiceCatalogIntegrationTests.cs` — full CRUD integration tests with Testcontainers
