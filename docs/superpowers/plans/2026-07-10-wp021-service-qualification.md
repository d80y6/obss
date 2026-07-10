# WP-021: Service Qualification (TMF645) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development or executing-plans.

**Goal:** Create a new ServiceQualification module implementing TMF645 with database-driven coverage maps.

**Architecture:** New 4-layer module under `src/Modules/ServiceQualification/`. Coverage-based engine queries seeded `CoverageArea` data.

**Tech Stack:** .NET 9, EF Core + Npgsql, MediatR, FluentValidation, Mapster, Next.js 16, React 19, Tailwind CSS 4

---

## File Structure

```
src/Modules/ServiceQualification/
  Obss.ServiceQualification.Domain/
    Entities/
      ServiceQualification.cs            # AggregateRoot<Guid>, state InProgress→Done|TerminatedWithError
      QualificationItem.cs               # Entity<Guid>, child of ServiceQualification
      AlternateServiceProposal.cs        # Entity<Guid>, child of QualificationItem
      CoverageArea.cs                    # Entity<Guid>, engine data (not aggregate)
    ValueObjects/
      GeographicAddress.cs               # Street, City, State?, PostalCode?, Country
      ServiceQualificationState.cs       # enum: InProgress, Done, TerminatedWithError
      QualificationResultType.cs         # enum: Qualified, Alternate, Unqualified, UnableToDetermine
      ServiceQualificationItemState.cs   # enum: InProgress, WaitingForInfo, Done, TerminatedWithError
      CoverageService.cs                 # ServiceName, SpeedMbps?, Technology, MonthlyPrice?, IsActive
    Events/
      ServiceQualificationSubmittedDomainEvent.cs
      ServiceQualificationCompletedDomainEvent.cs
      ServiceQualificationTerminatedDomainEvent.cs
    Obss.ServiceQualification.Domain.csproj

  Obss.ServiceQualification.Application/
    Abstractions/
      IServiceQualificationRepository.cs
      ICoverageAreaRepository.cs
      IServiceQualificationEngine.cs     # QualifyAsync(address, requestedServices) → QualificationEngineResult
    DTOs/
      ServiceQualificationDto.cs
      QualificationItemDto.cs
      AlternateProposalDto.cs
      GeographicAddressDto.cs
      CoverageAreaDto.cs
    Commands/CheckServiceQualification/
      CheckServiceQualificationCommand.cs
      CheckServiceQualificationCommandHandler.cs  # creates + runs engine + saves
      CheckServiceQualificationCommandValidator.cs
    Queries/GetServiceQualificationById/
      GetServiceQualificationByIdQuery.cs
      GetServiceQualificationByIdQueryHandler.cs
    Queries/GetServiceQualifications/
      GetServiceQualificationsQuery.cs
      GetServiceQualificationsQueryHandler.cs
    Mappings/
      ServiceQualificationMappingConfig.cs
    Services/
      CoverageBasedQualificationEngine.cs  # matches address → coverage areas → returns results
    Obss.ServiceQualification.Application.csproj

  Obss.ServiceQualification.Infrastructure/
    Persistence/
      ServiceQualificationDbContext.cs
      ServiceQualificationDbContextFactory.cs
      Configurations/
        ServiceQualificationConfiguration.cs
        QualificationItemConfiguration.cs
        CoverageAreaConfiguration.cs
      Repositories/
        ServiceQualificationRepository.cs
        CoverageAreaRepository.cs
    Obss.ServiceQualification.Infrastructure.csproj

  Obss.ServiceQualification.Api/
    Endpoints/ServiceQualificationEndpoints.cs
    Extensions/ServiceQualificationModuleRegistration.cs
    Obss.ServiceQualification.Api.csproj

frontend/
  src/api/hooks/useServiceQualification.ts
  src/app/service-qualification/check/page.tsx
  src/app/service-qualification/[id]/page.tsx
  src/app/service-qualification/page.tsx
  src/lib/query-keys.ts (modify)
```

---

### Task 1: Project Scaffolding

**Files:**
- Create: `src/Modules/ServiceQualification/Obss.ServiceQualification.Domain/Obss.ServiceQualification.Domain.csproj` — ref SharedKernel
- Create: `src/Modules/ServiceQualification/Obss.ServiceQualification.Application/Obss.ServiceQualification.Application.csproj` — ref Domain + MediatR/FluentValidation/Mapster
- Create: `src/Modules/ServiceQualification/Obss.ServiceQualification.Infrastructure/Obss.ServiceQualification.Infrastructure.csproj` — ref Application + EF Core/EFCore.NamingConventions
- Create: `src/Modules/ServiceQualification/Obss.ServiceQualification.Api/Obss.ServiceQualification.Api.csproj` — Sdk=Web, OutputType=Library, ref Application + Infrastructure
- Create: `src/Modules/ServiceQualification/Obss.ServiceQualification.Infrastructure/Persistence/ServiceQualificationDbContext.cs` — extend EfDbContext, ApplyConfigurationsFromAssembly
- Create: `src/Modules/ServiceQualification/Obss.ServiceQualification.Infrastructure/Persistence/ServiceQualificationDbContextFactory.cs` — IDesignTimeDbContextFactory with UseSnakeCaseNamingConvention
- Modify: `Obss.sln` — add 4 projects via `dotnet sln add`
- Modify: `src/Host/Obss.Host/Program.cs` — add 5 call-sites (DbContext, MediatR, Validators, AddServiceQualificationModule, MapServiceQualificationEndpoints)

- [ ] Create the 4 .csproj files following the standard module pattern
- [ ] Create DbContext and DbContextFactory
- [ ] `dotnet sln Obss.sln add` the 4 projects
- [ ] Register in Program.cs (5 call-sites after EventManagement entries)
- [ ] Build: `dotnet build --configuration Release -maxCpuCount:1` — expect success
- [ ] Commit: `git add -A && git commit -m "feat: scaffold ServiceQualification module"`

---

### Task 2: Domain — Enums and Value Objects

**Files:**
- Create: `Obss.ServiceQualification.Domain/ValueObjects/ServiceQualificationState.cs` — enum InProgress, Done, TerminatedWithError
- Create: `Obss.ServiceQualification.Domain/ValueObjects/QualificationResultType.cs` — enum Qualified, Alternate, Unqualified, UnableToDetermine
- Create: `Obss.ServiceQualification.Domain/ValueObjects/ServiceQualificationItemState.cs` — enum InProgress, WaitingForInfo, Done, TerminatedWithError
- Create: `Obss.ServiceQualification.Domain/ValueObjects/GeographicAddress.cs` — sealed class : ValueObject, factory Create() with validation, GetEqualityComponents for Street/City/State/PostalCode/Country
- Create: `Obss.ServiceQualification.Domain/ValueObjects/CoverageService.cs` — sealed class : ValueObject, ctor with ServiceName/SpeedMbps/Technology/MonthlyPrice/IsActive, GetEqualityComponents

- [ ] Create all 5 files with the code described above
- [ ] Build
- [ ] Commit: `git add -A && git commit -m "feat: add ServiceQualification enums and value objects"`

---

### Task 3: Domain — Domain Events

**Files:**
- Create: `Obss.ServiceQualification.Domain/Events/ServiceQualificationSubmittedDomainEvent.cs` — record(QualificationId, CustomerId) : DomainEvent
- Create: `Obss.ServiceQualification.Domain/Events/ServiceQualificationCompletedDomainEvent.cs` — record(QualificationId, CustomerId, IsFullyQualified) : DomainEvent
- Create: `Obss.ServiceQualification.Domain/Events/ServiceQualificationTerminatedDomainEvent.cs` — record(QualificationId, Reason) : DomainEvent

- [ ] Create all 3 files
- [ ] Build
- [ ] Commit

---

### Task 4: Domain — Entities

**Files:**
- Create: `AlternateServiceProposal.cs` — Entity<Guid>, props: ServiceId, ServiceName, ResultType(QualificationResultType), EstimatedInstallDate?, GuaranteedUntil?
- Create: `QualificationItem.cs` — Entity<Guid>, props: ServiceId, ServiceName, ResultType, State, EstimatedInstallDate?, EstimatedCompletionDate?, EligibilityUnavailableReason?, AlternateProposals collection. Methods: SetResult(), AddAlternateProposal()
- Create: `ServiceQualification.cs` — AggregateRoot<Guid>, props: CustomerId, Address(GeographicAddress), State, RequestedDate, ExpirationDate?, Description?, Items collection. Ctor fires SubmittedDomainEvent. Methods: AddItem(), Complete() fires CompletedDomainEvent, Terminate() fires TerminatedDomainEvent
- Create: `CoverageArea.cs` — Entity<Guid>, props: City, State?, StreetFrom?, StreetTo?, PostalCode?, AvailableServices(List<CoverageService>)

- [ ] Create all 4 entity files
- [ ] Build
- [ ] Commit

---

### Task 5: Application — Repository Interfaces, DTOs, and Mappings

**Files:**
- Create: `Abstractions/IServiceQualificationRepository.cs` — AddAsync, GetByIdAsync, GetByCustomerIdAsync
- Create: `Abstractions/ICoverageAreaRepository.cs` — GetByAddressAsync(address) returns matching CoverageAreas
- Create: `Abstractions/IServiceQualificationEngine.cs` — QualifyAsync(GeographicAddress, List<QualificationRequestItem>, CancellationToken) returns QualificationEngineResult. Also define QualificationRequestItem record and QualificationEngineResult/QualificationEngineItemResult records in same file.
- Create: `DTOs/ServiceQualificationDto.cs` — all DTOs in one file: ServiceQualificationDto, QualificationItemDto, AlternateProposalDto, GeographicAddressDto, CoverageAreaDto
- Create: `Mappings/ServiceQualificationMappingConfig.cs` — Mapster config: ServiceQualification → ServiceQualificationDto, etc.

- [ ] Create all files
- [ ] Build
- [ ] Commit

---

### Task 6: Application — Commands

**Files:**
- Create: `Commands/CheckServiceQualification/CheckServiceQualificationCommand.cs` — record with Address/RequestedServices/Description/CustomerId, implements IRequest<Result<ServiceQualificationDto>>
- Create: `Commands/CheckServiceQualification/CheckServiceQualificationCommandHandler.cs` — creates ServiceQualification aggregate, adds items, calls engine, updates items with results, calls Complete(), saves, returns DTO
- Create: `Commands/CheckServiceQualification/CheckServiceQualificationCommandValidator.cs` — FluentValidation: street/city/country required, at least one requested service, service name required

- [ ] Create all 3 files
- [ ] Build
- [ ] Commit

---

### Task 7: Application — Queries

**Files:**
- Create: `Queries/GetServiceQualificationById/GetServiceQualificationByIdQuery.cs` — record(Guid Id) : IRequest<Result<ServiceQualificationDto>>
- Create: `Queries/GetServiceQualificationById/GetServiceQualificationByIdQueryHandler.cs` — fetch by ID, map to DTO
- Create: `Queries/GetServiceQualifications/GetServiceQualificationsQuery.cs` — record(Guid? CustomerId, DateTime? From, DateTime? To, QualificationResultType? Result) : IRequest<Result<List<ServiceQualificationDto>>>
- Create: `Queries/GetServiceQualifications/GetServiceQualificationsQueryHandler.cs` — fetch with filters via repository

- [ ] Create all 4 files
- [ ] Build
- [ ] Commit

---

### Task 8: Application — CoverageBasedQualificationEngine

**Files:**
- Create: `Services/CoverageBasedQualificationEngine.cs` — implementation of IServiceQualificationEngine. Takes ICoverageAreaRepository via DI. Algorithm: lookup coverage areas matching address → for each requested service, find matching CoverageService → if exact match → Qualified, if different speed/tech → Alternate, if none → Unqualified. Returns QualificationEngineResult.

- [ ] Create file
- [ ] Build
- [ ] Commit

---

### Task 9: Infrastructure — EF Configurations

**Files:**
- Create: `Persistence/Configurations/ServiceQualificationConfiguration.cs` — IEntityTypeConfiguration<ServiceQualification>: OwnsOne GeographicAddress as columns (address_street, address_city, address_state, address_postal_code, address_country), OwnsMany QualificationItem
- Create: `Persistence/Configurations/QualificationItemConfiguration.cs` — IEntityTypeConfiguration<QualificationItem>: OwnsMany AlternateServiceProposal
- Create: `Persistence/Configurations/CoverageAreaConfiguration.cs` — IEntityTypeConfiguration<CoverageArea>: OwnsMany CoverageService as columns (service_name, speed_mbps, technology, monthly_price, is_active). Seed sample data: Sana'a (fiber 1000Mbps, DSL 20Mbps), Aden (fiber 500Mbps, wireless 50Mbps), Taiz (DSL 10Mbps)

- [ ] Create all 3 configuration files
- [ ] Build
- [ ] Commit

---

### Task 10: Infrastructure — Repositories

**Files:**
- Create: `Persistence/Repositories/ServiceQualificationRepository.cs` — implement IServiceQualificationRepository using EF Core, include Items then AlternateProposals via ThenInclude
- Create: `Persistence/Repositories/CoverageAreaRepository.cs` — implement ICoverageAreaRepository: match by city + (state optional) + (postal code optional) + street range check

- [ ] Create both repository files
- [ ] Build
- [ ] Commit

---

### Task 11: Infrastructure — Migration

- [ ] Generate EF migration: `dotnet ef migrations add AddServiceQualificationTables -p src/Modules/ServiceQualification/Obss.ServiceQualification.Infrastructure -s src/Host/Obss.Host -c ServiceQualificationDbContext`
- [ ] Build
- [ ] Commit: `git add -A && git commit -m "feat: add ServiceQualification database migration"`

---

### Task 12: API — Endpoints and Module Registration

**Files:**
- Create: `Extensions/ServiceQualificationModuleRegistration.cs` — AddServiceQualificationModule: register IRepo→Repo, IEngine→Engine, services.AddScoped<IRepository<>>, Mapster config. MapServiceQualificationEndpoints: group /api/v{version}/service-qualification
- Create: `Endpoints/ServiceQualificationEndpoints.cs` — POST / (CheckServiceQualification), GET /{id} (GetServiceQualificationById), GET / (GetServiceQualifications)

- [ ] Create both files
- [ ] Build
- [ ] Commit

---

### Task 13: Frontend — Hooks and Query Keys

**Files:**
- Modify: `src/lib/query-keys.ts` — add serviceQualification query key factory (all, byId, list, byCustomer)
- Create: `src/api/hooks/useServiceQualification.ts` — useCheckServiceQualification mutation, useServiceQualification(id) query, useServiceQualifications(filters) query

- [ ] Create/modify files
- [ ] Build: `bun run build` or check Next.js typecheck
- [ ] Commit

---

### Task 14: Frontend — Pages

**Files:**
- Create: `src/app/service-qualification/check/page.tsx` — form with street/city/state/zip/country inputs + multi-service selector + submit button. Calls useCheckServiceQualification, navigates to result on success
- Create: `src/app/service-qualification/[id]/page.tsx` — result display: address summary, per-item status (badge: qualified green/alternate yellow/unqualified red), alternate proposals list, install estimates
- Create: `src/app/service-qualification/page.tsx` — list page with filter controls (customer, date range, result type) and table of past checks

- [ ] Create all 3 pages
- [ ] Verify build
- [ ] Commit

---

### Task 15: Full Build and Format Verification

- [ ] `dotnet build --configuration Release -maxCpuCount:1` — 0 errors, 0 warnings
- [ ] `bun run lint` (or equivalent frontend check)
- [ ] `dotnet format --verify-no-changes --verbosity diagnostic` — no formatting issues
- [ ] Commit any final fixes
