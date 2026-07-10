# WP-021: Service Qualification (TMF645) — Design Spec

**Date:** 2026-07-10
**Module:** `Obss.ServiceQualification` (new)
**TMF API:** TMF645 Service Qualification Management
**Approach:** Database-driven coverage maps (Approach 2)

---

## 1. Domain Model

### 1.1 Entities

**`ServiceQualification`** (AggregateRoot\<Guid>)
- State machine: `InProgress → Done | TerminatedWithError`
- Properties: `RequestedDate`, `ExpirationDate`, `Description`, `CustomerId`
- Collections: `Items` (List\<QualificationItem>)
- Owns: `GeographicAddress` (the location being qualified)
- References: `CustomerId` (Guid) — the customer who requested qualification (RelatedParty simplified for MVP)

**`QualificationItem`** (Entity\<Guid>)
- Properties: `ServiceId`, `ServiceName`, `QualificationResult`, `State`, `EstimatedInstallDate`, `EstimatedCompletionDate`, `EligibilityUnavailableReason`
- Collections: `AlternateProposals` (List\<AlternateServiceProposal>)

**`AlternateServiceProposal`** (Entity)
- Properties: `ServiceId`, `ServiceName`, `QualificationResult`, `EstimatedInstallDate`, `GuaranteedUntil`

### 1.2 Value Objects

**`GeographicAddress`** (ValueObject)
- `Street` (string), `City` (string), `State` (string?), `PostalCode` (string?), `Country` (string)
- Equality via all components

**`ServiceQualificationState`** (enum): `InProgress`, `Done`, `TerminatedWithError`

**`QualificationResultType`** (enum): `Qualified`, `Alternate`, `Unqualified`, `UnableToDetermine`

**`ServiceQualificationItemState`** (enum): `InProgress`, `WaitingForInfo`, `Done`, `TerminatedWithError`

### 1.3 Coverage Tables (Engine Data)

**`CoverageArea`** (Entity, non-aggregate - engine data only)
- Properties: `City`, `State`, `PostalCode` (nullable), `StreetFrom` (nullable), `StreetTo` (nullable)
- Collections: `AvailableServices` (List\<CoverageService>)

**`CoverageService`** (ValueObject)
- Properties: `ServiceName`, `SpeedMbps` (int?), `Technology` (string), `MonthlyPrice` (decimal?), `IsActive`

### 1.4 Domain Events

- `ServiceQualificationSubmittedDomainEvent`
- `ServiceQualificationCompletedDomainEvent`
- `ServiceQualificationTerminatedDomainEvent`

---

## 2. Application Layer

### 2.1 Commands & Queries

| Operation | Endpoint | Handler | Validator |
|-----------|----------|---------|-----------|
| CheckServiceQualification | POST `/api/v1/service-qualification` | `CheckServiceQualificationCommandHandler` | `CheckServiceQualificationCommandValidator` |
| GetServiceQualificationById | GET `/api/v1/service-qualification/{id}` | `GetServiceQualificationByIdQueryHandler` | — |
| GetServiceQualifications | GET `/api/v1/service-qualification` | `GetServiceQualificationsQueryHandler` | — |

**Validator rules (CheckServiceQualification):** Address fields (street, city, country required), at least one requested service, service name required.

### 2.2 Engine Interface

```csharp
public interface IServiceQualificationEngine
{
    Task<QualificationEngineResult> QualifyAsync(
        GeographicAddress address,
        IReadOnlyList<QualificationRequestItem> requestedServices,
        CancellationToken cancellationToken);
}
```

`CoverageBasedQualificationEngine` implementation:
1. Normalize address (lowercase, trim)
2. Query `CoverageArea` matching city + state + postal code + street range
3. For each requested service, check if any `CoverageService` matches
4. Return `QualificationEngineResult` with per-item results: qualified (exact match), alternate (different speed/tech), or unqualified (no match)

`QualificationEngineResult` contains `List<QualificationEngineItemResult>` with `ServiceName`, `ResultType`, and `List<AlternateServiceProposal>`.

### 2.3 DTOs

- `ServiceQualificationDto` — full qualification result
- `QualificationItemDto` — per-item result
- `AlternateProposalDto` — alternative service details
- `GeographicAddressDto` — address data
- `CoverageAreaDto` — coverage area with available services

### 2.4 Mapster Mappings

`ServiceQualificationMappingConfig` — maps domain entities to DTOs, including nested owned collections.

---

## 3. Infrastructure Layer

### 3.1 Database Schema: `service_qualification`

| Table | Columns | Notes |
|-------|---------|-------|
| `service_qualifications` | id, customer_id, requested_date, expiration_date, description, state, street, city, state_name, postal_code, country, created_at, updated_at | `state_name` avoids SQL keyword conflict |
| `service_qualification_items` | id, qualification_id, service_id, service_name, qualification_result, item_state, estimated_install_date, estimated_completion_date, reason | FK → service_qualifications |
| `service_qualification_item_alternatives` | id, item_id, service_id, service_name, qualification_result, estimated_install_date, guaranteed_until | FK → service_qualification_items |
| `coverage_areas` | id, city, state_name, postal_code, street_from, street_to | sample seed data |
| `coverage_area_services` | id, coverage_area_id, service_name, speed_mbps, technology, monthly_price, is_active | FK → coverage_areas |

### 3.2 EF Configurations

- `ServiceQualificationConfiguration` — OwnsOne `GeographicAddress` as columns, OwnsMany `QualificationItem`
- `QualificationItemConfiguration` — OwnsMany `AlternateServiceProposal`
- `CoverageAreaConfiguration` — OwnsMany `CoverageService` + seed data

### 3.3 Seed Data

Sample coverage areas for 3 cities with fiber/DSL/wireless at various speeds — provides realistic qualification results during development.

### 3.4 DbContext

`ServiceQualificationDbContext` extending `EfDbContext`, with `ApplyConfigurationsFromAssembly`.

### 3.5 Design-Time Factory

`ServiceQualificationDbContextFactory` for EF migrations.

### 3.6 Repositories

- `ServiceQualificationRepository` implementing `IServiceQualificationRepository`
- `CoverageAreaRepository` implementing `ICoverageAreaRepository`

---

## 4. API Layer

### 4.1 Endpoints

```
POST   /api/v1/service-qualification          → CheckServiceQualification
GET    /api/v1/service-qualification/{id}      → GetServiceQualificationById
GET    /api/v1/service-qualification           → GetServiceQualifications (list)
```

### 4.2 Module Registration

`ServiceQualificationModuleRegistration` with:
- `AddServiceQualificationModule(this IServiceCollection)` — DI for repositories, engine, hosted services
- `MapServiceQualificationEndpoints(this IEndpointRouteBuilder)` — endpoint group under `/api/v{version:apiVersion}/service-qualification`

### 4.3 Program.cs Registration

Five call-sites following the established pattern:
1. `AddModuleDbContext<ServiceQualificationDbContext>("service_qualification")`
2. `RegisterServicesFromAssembly(typeof(CheckServiceQualificationCommand).Assembly)`
3. `AddValidatorsFromAssembly(...)` (if validators needed)
4. `builder.Services.AddServiceQualificationModule()`
5. `app.MapServiceQualificationEndpoints()`

---

## 5. Frontend

### 5.1 Pages

| Route | Purpose |
|-------|---------|
| `/service-qualification/check` | Form: address fields + service selection → submit check |
| `/service-qualification/[id]` | Result page: per-item status, alternate proposals, install estimates |
| `/service-qualification/` | List page: past qualification checks with filters |

### 5.2 Components

- `QualificationForm` — address input + multi-service selector
- `QualificationResultCard` — per-item result display (qualified green / alternate yellow / unqualified red)
- `AlternateProposalList` — alternate service offers with accept/compare actions
- `QualificationHistoryTable` — past checks with status badges

### 5.3 Hooks & Query Keys

- `useServiceQualification` — check + retrieve operations
- Query keys under `serviceQualification` namespace

---

## 6. Implementation Order

1. Project scaffolding (4 .csproj files, solution entry, Program.cs registration)
2. Domain: enums, value objects, domain events
3. Domain: ServiceQualification aggregate root, QualificationItem entity
4. Application: repository interfaces, DTOs, Mapster mappings
5. Application: commands + handlers
6. Application: queries + handlers
7. Application: IServiceQualificationEngine interface + CoverageBasedQualificationEngine
8. Infrastructure: EF configurations + DbContext
9. Infrastructure: repositories
10. Infrastructure: seed data + migration
11. API: endpoints + module registration
12. Frontend: hooks, query keys, pages
13. Build verification
