# TMF633 Service Catalog — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a complete TMF633 Service Catalog Management module as a standalone 4-layer module following existing Clean Architecture patterns, with full frontend support.

**Architecture:** Standalone module `Obss.ServiceCatalog` (Api/Application/Domain/Infrastructure) following existing patterns from ProductCatalog. Integrates with ProductCatalog, ServiceInventory, and Provisioning via foreign keys to `ServiceSpecification`.

**Tech Stack:** .NET 9, ASP.NET Core, EF Core/Npgsql, MediatR, FluentValidation, Mapster, Next.js 16, React 19, TanStack Query

---

## File Structure

### New files (~70):

```
src/Modules/ServiceCatalog/
├── Obss.ServiceCatalog.Domain/
│   ├── Obss.ServiceCatalog.Domain.csproj
│   ├── Entities/
│   │   ├── ServiceCategory.cs
│   │   ├── ServiceCandidate.cs
│   │   ├── ServiceSpecification.cs
│   │   ├── ServiceSpecCharacteristic.cs
│   │   ├── ServiceSpecCharValue.cs
│   │   └── ServiceSpecRelationship.cs
│   ├── Enums/
│   │   ├── LifecycleStatus.cs
│   │   └── RelationshipType.cs
│   └── Exceptions/
│       └── ServiceCatalogDomainException.cs
├── Obss.ServiceCatalog.Application/
│   ├── Obss.ServiceCatalog.Application.csproj
│   ├── packages.lock.json
│   ├── Abstractions/
│   │   ├── IServiceCategoryRepository.cs
│   │   ├── IServiceCandidateRepository.cs
│   │   └── IServiceSpecificationRepository.cs
│   ├── DTOs/
│   │   ├── ServiceCategoryDto.cs
│   │   ├── ServiceCandidateDto.cs
│   │   ├── ServiceSpecificationDto.cs
│   │   ├── ServiceSpecCharacteristicDto.cs
│   │   ├── ServiceSpecCharValueDto.cs
│   │   └── ServiceSpecRelationshipDto.cs
│   ├── Commands/ServiceCategory/
│   │   ├── CreateServiceCategory/
│   │   ├── UpdateServiceCategory/
│   │   └── DeleteServiceCategory/
│   ├── Commands/ServiceCandidate/
│   │   ├── CreateServiceCandidate/
│   │   ├── UpdateServiceCandidate/
│   │   └── DeleteServiceCandidate/
│   ├── Commands/ServiceSpecification/
│   │   ├── CreateServiceSpecification/
│   │   ├── UpdateServiceSpecification/
│   │   ├── DeleteServiceSpecification/
│   │   ├── AddCharacteristic/
│   │   ├── RemoveCharacteristic/
│   │   ├── UpdateCharacteristic/
│   │   ├── AddCharacteristicValue/
│   │   ├── RemoveCharacteristicValue/
│   │   ├── UpdateCharacteristicValue/
│   │   ├── AddSpecRelationship/
│   │   └── RemoveSpecRelationship/
│   ├── Queries/
│   │   ├── GetServiceCategories/
│   │   ├── GetServiceCategoryById/
│   │   ├── GetServiceCandidates/
│   │   ├── GetServiceCandidateById/
│   │   ├── GetServiceSpecifications/
│   │   ├── GetServiceSpecificationById/
│   │   ├── GetCharacteristics/
│   │   ├── GetCharacteristicValues/
│   │   └── GetSpecRelationships/
│   └── Mappings/
│       └── ServiceCatalogMappingConfig.cs
├── Obss.ServiceCatalog.Infrastructure/
│   ├── Obss.ServiceCatalog.Infrastructure.csproj
│   ├── packages.lock.json
│   ├── DependencyInjection.cs
│   └── Persistence/
│       ├── ServiceCatalogDbContext.cs
│       ├── ServiceCatalogDbContextFactory.cs
│       ├── Configurations/
│       │   ├── ServiceCategoryConfiguration.cs
│       │   ├── ServiceCandidateConfiguration.cs
│       │   ├── ServiceSpecificationConfiguration.cs
│       │   ├── ServiceSpecCharacteristicConfiguration.cs
│       │   ├── ServiceSpecCharValueConfiguration.cs
│       │   └── ServiceSpecRelationshipConfiguration.cs
│       └── Repositories/
│           ├── ServiceCategoryRepository.cs
│           ├── ServiceCandidateRepository.cs
│           └── ServiceSpecificationRepository.cs
└── Obss.ServiceCatalog.Api/
    ├── Obss.ServiceCatalog.Api.csproj
    ├── packages.lock.json
    ├── Endpoints/
    │   ├── ServiceCategoryEndpoints.cs
    │   ├── ServiceCandidateEndpoints.cs
    │   └── ServiceSpecificationEndpoints.cs
    └── Extensions/
        └── ServiceCatalogRegistration.cs
```

### Modified files:
- `Obss.sln` — add 4 new projects
- `src/Host/Obss.Host/Modules/ModuleLoader.cs` — register new module
- `src/Host/Obss.Host/Program.cs` — call module registration
- ProductCatalog entities — add `ServiceSpecificationId` optional FK
- ServiceInventory entities — add `ServiceSpecificationId` optional FK
- Provisioning entities — add `ServiceSpecificationId` optional FK

### Test files (~5):
- `tests/Modules/ServiceCatalog.Tests/ServiceCatalogIntegrationTests.csproj`
- `tests/Modules/ServiceCatalog.Tests/CommandHandlerTests.cs`
- `tests/Modules/ServiceCatalog.Tests/RepositoryTests.cs`
- `tests/Modules/ServiceCatalog.Tests/ServiceCatalogIntegrationTests.cs`

### Frontend files (~15):
- `frontend/src/app/service-catalog/` — pages
- `frontend/src/api/hooks/use-service-catalog.ts` — hooks

---

### Task 1: Module Project Scaffolding

**Files:**
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Obss.ServiceCatalog.Domain.csproj`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Obss.ServiceCatalog.Application.csproj`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Obss.ServiceCatalog.Infrastructure.csproj`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Api/Obss.ServiceCatalog.Api.csproj`
- Modify: `Obss.sln`

- [ ] **Step 1: Create Domain project**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <RootNamespace>Obss.ServiceCatalog.Domain</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\..\Shared\Obss.SharedKernel\Obss.SharedKernel.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Create Application project**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <RootNamespace>Obss.ServiceCatalog.Application</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Obss.ServiceCatalog.Domain\Obss.ServiceCatalog.Domain.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Create Infrastructure project**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <RootNamespace>Obss.ServiceCatalog.Infrastructure</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Obss.ServiceCatalog.Application\Obss.ServiceCatalog.Application.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Create Api project**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <RootNamespace>Obss.ServiceCatalog.Api</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Obss.ServiceCatalog.Application\Obss.ServiceCatalog.Application.csproj" />
    <ProjectReference Include="..\Obss.ServiceCatalog.Infrastructure\Obss.ServiceCatalog.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 5: Add projects to solution**

Run:
```bash
dotnet sln Obss.sln add src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Obss.ServiceCatalog.Domain.csproj
dotnet sln Obss.sln add src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Obss.ServiceCatalog.Application.csproj
dotnet sln Obss.sln add src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Obss.ServiceCatalog.Infrastructure.csproj
dotnet sln Obss.sln add src/Modules/ServiceCatalog/Obss.ServiceCatalog.Api/Obss.ServiceCatalog.Api.csproj
```

- [ ] **Step 6: Verify solution builds**

Run:
```bash
dotnet build Obss.sln --no-restore 2>&1 | tail -5
```
Expected: Build succeeded with 0 warnings

- [ ] **Step 7: Commit**

```bash
git add Obss.sln src/Modules/ServiceCatalog/
git commit -m "feat(service-catalog): scaffold TMF633 module projects"
```

---

### Task 2: Domain Layer — Enums, Exceptions, Value Objects

**Files:**
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Enums/LifecycleStatus.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Enums/RelationshipType.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Exceptions/ServiceCatalogDomainException.cs`

- [ ] **Step 1: Create LifecycleStatus enum**

```csharp
namespace Obss.ServiceCatalog.Domain.Enums;

public enum LifecycleStatus
{
    Draft,
    Active,
    Retired
}
```

- [ ] **Step 2: Create RelationshipType enum**

```csharp
namespace Obss.ServiceCatalog.Domain.Enums;

public enum RelationshipType
{
    ReliesOn,
    DependsOn,
    Aggregates,
    IsConnectedTo
}
```

- [ ] **Step 3: Create domain exception**

```csharp
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.ServiceCatalog.Domain.Exceptions;

[Serializable]
public class ServiceCatalogDomainException : DomainException
{
    public ServiceCatalogDomainException() { }
    public ServiceCatalogDomainException(string message) : base(message) { }
    public ServiceCatalogDomainException(string message, Exception inner) : base(message, inner) { }
    protected ServiceCatalogDomainException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
```

- [ ] **Step 4: Commit**

```bash
git add src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/
git commit -m "feat(service-catalog): add domain enums and exceptions"
```

---

### Task 3: Domain Entities — ServiceCategory and ServiceCandidate

**Files:**
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Entities/ServiceCategory.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Entities/ServiceCandidate.cs`

- [ ] **Step 1: Create ServiceCategory aggregate**

```csharp
using Obss.ServiceCatalog.Domain.Enums;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceCatalog.Domain.Entities;

public class ServiceCategory : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<ServiceCandidate> _candidates = [];

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public LifecycleStatus LifecycleStatus { get; private set; }
    public int Version { get; private set; } = 1;
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsRoot => !ParentCategoryId.HasValue;
    public IReadOnlyCollection<ServiceCandidate> Candidates => _candidates.AsReadOnly();

    private ServiceCategory() { }

    private ServiceCategory(Guid id, string tenantId, string name, string? description, Guid? parentCategoryId, DateTime? validFrom, DateTime? validTo) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;
        LifecycleStatus = LifecycleStatus.Draft;
        ValidFrom = validFrom;
        ValidTo = validTo;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static ServiceCategory Create(string tenantId, string name, string? description = null, Guid? parentCategoryId = null, DateTime? validFrom = null, DateTime? validTo = null)
    {
        return new ServiceCategory(Guid.NewGuid(), tenantId, name, description, parentCategoryId, validFrom, validTo);
    }

    public void Activate()
    {
        LifecycleStatus = LifecycleStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Retire()
    {
        LifecycleStatus = LifecycleStatus.Retired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetValidityPeriod(DateTime? validFrom, DateTime? validTo)
    {
        ValidFrom = validFrom;
        ValidTo = validTo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCandidate(ServiceCandidate candidate)
    {
        if (!_candidates.Any(c => c.Id == candidate.Id))
            _candidates.Add(candidate);
    }

    public void RemoveCandidate(Guid candidateId)
    {
        _candidates.RemoveAll(c => c.Id == candidateId);
    }
}
```

- [ ] **Step 2: Create ServiceCandidate aggregate**

```csharp
using Obss.ServiceCatalog.Domain.Enums;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceCatalog.Domain.Entities;

public class ServiceCandidate : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<ServiceCategory> _categories = [];

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public LifecycleStatus LifecycleStatus { get; private set; }
    public int Version { get; private set; } = 1;
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public Guid? ServiceSpecificationId { get; private set; }
    public Guid? BaseCandidateId { get; private set; }
    public string? FeatureSpecification { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<ServiceCategory> Categories => _categories.AsReadOnly();

    private ServiceCandidate() { }

    private ServiceCandidate(Guid id, string tenantId, string name, string? description, Guid? serviceSpecificationId, Guid? baseCandidateId, string? featureSpecification, DateTime? validFrom, DateTime? validTo) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ServiceSpecificationId = serviceSpecificationId;
        BaseCandidateId = baseCandidateId;
        FeatureSpecification = featureSpecification;
        LifecycleStatus = LifecycleStatus.Draft;
        ValidFrom = validFrom;
        ValidTo = validTo;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static ServiceCandidate Create(string tenantId, string name, string? description = null, Guid? serviceSpecificationId = null, Guid? baseCandidateId = null, string? featureSpecification = null, DateTime? validFrom = null, DateTime? validTo = null)
    {
        return new ServiceCandidate(Guid.NewGuid(), tenantId, name, description, serviceSpecificationId, baseCandidateId, featureSpecification, validFrom, validTo);
    }

    public void Activate()
    {
        LifecycleStatus = LifecycleStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Retire()
    {
        LifecycleStatus = LifecycleStatus.Retired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description, string? featureSpecification)
    {
        Name = name;
        Description = description;
        FeatureSpecification = featureSpecification;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignSpecification(Guid serviceSpecificationId)
    {
        ServiceSpecificationId = serviceSpecificationId;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

- [ ] **Step 3: Verify compilation**

```bash
dotnet build src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Obss.ServiceCatalog.Domain.csproj 2>&1 | tail -3
```

- [ ] **Step 4: Commit**

```bash
git add src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/
git commit -m "feat(service-catalog): add ServiceCategory and ServiceCandidate entities"
```

---

### Task 4: Domain Entities — ServiceSpecification and sub-entities

**Files:**
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Entities/ServiceSpecification.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Entities/ServiceSpecCharacteristic.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Entities/ServiceSpecCharValue.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Entities/ServiceSpecRelationship.cs`

- [ ] **Step 1: Create ServiceSpecification aggregate**

```csharp
using Obss.ServiceCatalog.Domain.Enums;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceCatalog.Domain.Entities;

public class ServiceSpecification : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<ServiceSpecCharacteristic> _characteristics = [];
    private readonly List<ServiceSpecRelationship> _relationships = [];

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Brand { get; private set; }
    public string? Version { get; private set; }
    public LifecycleStatus LifecycleStatus { get; private set; }
    public bool IsBundle { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<ServiceSpecCharacteristic> Characteristics => _characteristics.AsReadOnly();
    public IReadOnlyCollection<ServiceSpecRelationship> Relationships => _relationships.AsReadOnly();

    private ServiceSpecification() { }

    private ServiceSpecification(Guid id, string tenantId, string name, string? description, string? brand, string? version, bool isBundle, DateTime? validFrom, DateTime? validTo) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        Brand = brand;
        Version = version;
        IsBundle = isBundle;
        LifecycleStatus = LifecycleStatus.Draft;
        ValidFrom = validFrom;
        ValidTo = validTo;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static ServiceSpecification Create(string tenantId, string name, string? description = null, string? brand = null, string? version = null, bool isBundle = false, DateTime? validFrom = null, DateTime? validTo = null)
    {
        return new ServiceSpecification(Guid.NewGuid(), tenantId, name, description, brand, version, isBundle, validFrom, validTo);
    }

    public void Activate()
    {
        LifecycleStatus = LifecycleStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Retire()
    {
        LifecycleStatus = LifecycleStatus.Retired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description, string? brand, string? version)
    {
        Name = name;
        Description = description;
        Brand = brand;
        Version = version;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetValidityPeriod(DateTime? validFrom, DateTime? validTo)
    {
        ValidFrom = validFrom;
        ValidTo = validTo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCharacteristic(ServiceSpecCharacteristic characteristic)
    {
        _characteristics.Add(characteristic);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveCharacteristic(Guid characteristicId)
    {
        _characteristics.RemoveAll(c => c.Id == characteristicId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRelationship(ServiceSpecRelationship relationship)
    {
        _relationships.Add(relationship);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRelationship(Guid relationshipId)
    {
        _relationships.RemoveAll(r => r.Id == relationshipId);
        UpdatedAt = DateTime.UtcNow;
    }
}
```

- [ ] **Step 2: Create ServiceSpecCharacteristic entity**

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceCatalog.Domain.Entities;

public class ServiceSpecCharacteristic : Entity<Guid>
{
    private readonly List<ServiceSpecCharValue> _values = [];

    public Guid ServiceSpecificationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ValueType { get; private set; } = "string";
    public bool Configurable { get; private set; } = true;
    public decimal? MinValue { get; private set; }
    public decimal? MaxValue { get; private set; }
    public string? Regex { get; private set; }
    public int SortOrder { get; private set; }
    public int? MaxCardinality { get; private set; }
    public bool IsRequired { get; private set; }
    public IReadOnlyCollection<ServiceSpecCharValue> Values => _values.AsReadOnly();

    private ServiceSpecCharacteristic() { }

    public ServiceSpecCharacteristic(Guid id, Guid serviceSpecificationId, string name, string? description, string valueType, bool configurable, decimal? minValue, decimal? maxValue, string? regex, int sortOrder, int? maxCardinality, bool isRequired) : base(id)
    {
        ServiceSpecificationId = serviceSpecificationId;
        Name = name;
        Description = description;
        ValueType = valueType;
        Configurable = configurable;
        MinValue = minValue;
        MaxValue = maxValue;
        Regex = regex;
        SortOrder = sortOrder;
        MaxCardinality = maxCardinality;
        IsRequired = isRequired;
    }

    public void UpdateDetails(string name, string? description, string valueType, bool configurable, decimal? minValue, decimal? maxValue, string? regex, int sortOrder, int? maxCardinality, bool isRequired)
    {
        Name = name;
        Description = description;
        ValueType = valueType;
        Configurable = configurable;
        MinValue = minValue;
        MaxValue = maxValue;
        Regex = regex;
        SortOrder = sortOrder;
        MaxCardinality = maxCardinality;
        IsRequired = isRequired;
    }

    public void AddValue(ServiceSpecCharValue value)
    {
        _values.Add(value);
    }

    public void RemoveValue(Guid valueId)
    {
        _values.RemoveAll(v => v.Id == valueId);
    }
}
```

- [ ] **Step 3: Create ServiceSpecCharValue entity**

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceCatalog.Domain.Entities;

public class ServiceSpecCharValue : Entity<Guid>
{
    public Guid CharacteristicId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public string? UnitOfMeasure { get; private set; }
    public bool IsDefault { get; private set; }
    public string? ValueFrom { get; private set; }
    public string? ValueTo { get; private set; }
    public string? RangeInterval { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    private ServiceSpecCharValue() { }

    public ServiceSpecCharValue(Guid id, Guid characteristicId, string value, string? unitOfMeasure, bool isDefault, string? valueFrom, string? valueTo, string? rangeInterval, DateTime? validFrom, DateTime? validTo) : base(id)
    {
        CharacteristicId = characteristicId;
        Value = value;
        UnitOfMeasure = unitOfMeasure;
        IsDefault = isDefault;
        ValueFrom = valueFrom;
        ValueTo = valueTo;
        RangeInterval = rangeInterval;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public void Update(string value, string? unitOfMeasure, bool isDefault, string? valueFrom, string? valueTo, string? rangeInterval, DateTime? validFrom, DateTime? validTo)
    {
        Value = value;
        UnitOfMeasure = unitOfMeasure;
        IsDefault = isDefault;
        ValueFrom = valueFrom;
        ValueTo = valueTo;
        RangeInterval = rangeInterval;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }
}
```

- [ ] **Step 4: Create ServiceSpecRelationship entity**

```csharp
using Obss.ServiceCatalog.Domain.Enums;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceCatalog.Domain.Entities;

public class ServiceSpecRelationship : Entity<Guid>
{
    public Guid ServiceSpecificationId { get; private set; }
    public Guid TargetSpecificationId { get; private set; }
    public RelationshipType RelationshipType { get; private set; }
    public string? Role { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    private ServiceSpecRelationship() { }

    public ServiceSpecRelationship(Guid id, Guid serviceSpecificationId, Guid targetSpecificationId, RelationshipType relationshipType, string? role, DateTime? validFrom, DateTime? validTo) : base(id)
    {
        ServiceSpecificationId = serviceSpecificationId;
        TargetSpecificationId = targetSpecificationId;
        RelationshipType = relationshipType;
        Role = role;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }
}
```

- [ ] **Step 5: Verify compilation**

```bash
dotnet build src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Obss.ServiceCatalog.Domain.csproj 2>&1 | tail -3
```

- [ ] **Step 6: Commit**

```bash
git add src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/
git commit -m "feat(service-catalog): add ServiceSpecification and sub-entities"
```

---

### Task 5: Application Layer — Abstractions and DTOs

**Files:**
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Abstractions/IServiceCategoryRepository.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Abstractions/IServiceCandidateRepository.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Abstractions/IServiceSpecificationRepository.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/DTOs/ServiceCategoryDto.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/DTOs/ServiceCandidateDto.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/DTOs/ServiceSpecificationDto.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/DTOs/ServiceSpecCharacteristicDto.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/DTOs/ServiceSpecCharValueDto.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/DTOs/ServiceSpecRelationshipDto.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Mappings/ServiceCatalogMappingConfig.cs`

- [ ] **Step 1: Create IServiceCategoryRepository**

```csharp
using Obss.ServiceCatalog.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceCatalog.Application.Abstractions;

public interface IServiceCategoryRepository : IRepository<ServiceCategory>
{
    Task<List<ServiceCategory>> GetRootCategoriesAsync(string tenantId, CancellationToken ct = default);
    Task<List<ServiceCategory>> GetChildCategoriesAsync(Guid parentId, CancellationToken ct = default);
    Task<ServiceCategory?> GetByIdWithCandidatesAsync(Guid id, CancellationToken ct = default);
}
```

- [ ] **Step 2: Create IServiceCandidateRepository**

```csharp
using Obss.ServiceCatalog.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceCatalog.Application.Abstractions;

public interface IServiceCandidateRepository : IRepository<ServiceCandidate>
{
    Task<List<ServiceCandidate>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<ServiceCandidate?> GetByIdWithCategoriesAsync(Guid id, CancellationToken ct = default);
}
```

- [ ] **Step 3: Create IServiceSpecificationRepository**

```csharp
using Obss.ServiceCatalog.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceCatalog.Application.Abstractions;

public interface IServiceSpecificationRepository : IRepository<ServiceSpecification>
{
    Task<ServiceSpecification?> GetByIdWithCharacteristicsAsync(Guid id, CancellationToken ct = default);
    Task<List<ServiceSpecification>> GetByBrandAsync(string brand, CancellationToken ct = default);
}
```

- [ ] **Step 4: Create DTOs**

ServiceCategoryDto.cs:
```csharp
namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceCategoryDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string LifecycleStatus,
    int Version,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsRoot
);
```

ServiceCandidateDto.cs:
```csharp
namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceCandidateDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string LifecycleStatus,
    int Version,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    Guid? ServiceSpecificationId,
    string? ServiceSpecificationName,
    Guid? BaseCandidateId,
    string? FeatureSpecification,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ServiceCategoryDto> Categories
);
```

ServiceSpecificationDto.cs:
```csharp
namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceSpecificationDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string? Brand,
    string? Version,
    string LifecycleStatus,
    bool IsBundle,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ServiceSpecCharacteristicDto> Characteristics,
    List<ServiceSpecRelationshipDto> Relationships
);
```

ServiceSpecCharacteristicDto.cs:
```csharp
namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceSpecCharacteristicDto(
    Guid Id,
    Guid ServiceSpecificationId,
    string Name,
    string? Description,
    string ValueType,
    bool Configurable,
    decimal? MinValue,
    decimal? MaxValue,
    string? Regex,
    int SortOrder,
    int? MaxCardinality,
    bool IsRequired,
    List<ServiceSpecCharValueDto> Values
);
```

ServiceSpecCharValueDto.cs:
```csharp
namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceSpecCharValueDto(
    Guid Id,
    Guid CharacteristicId,
    string Value,
    string? UnitOfMeasure,
    bool IsDefault,
    string? ValueFrom,
    string? ValueTo,
    string? RangeInterval,
    DateTime? ValidFrom,
    DateTime? ValidTo
);
```

ServiceSpecRelationshipDto.cs:
```csharp
namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceSpecRelationshipDto(
    Guid Id,
    Guid ServiceSpecificationId,
    Guid TargetSpecificationId,
    string RelationshipType,
    string? Role,
    DateTime? ValidFrom,
    DateTime? ValidTo
);
```

- [ ] **Step 5: Create Mapster mapping config**

```csharp
using Mapster;
using Obss.ServiceCatalog.Domain.Entities;
using Obss.ServiceCatalog.Domain.Enums;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Mappings;

public static class ServiceCatalogMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<ServiceCategory, ServiceCategoryDto>.NewConfig()
            .Map(dest => dest.LifecycleStatus, src => src.LifecycleStatus.ToString());

        TypeAdapterConfig<ServiceCandidate, ServiceCandidateDto>.NewConfig()
            .Map(dest => dest.LifecycleStatus, src => src.LifecycleStatus.ToString())
            .Map(dest => dest.ServiceSpecificationName, src => null); // populated by query handler

        TypeAdapterConfig<ServiceSpecification, ServiceSpecificationDto>.NewConfig()
            .Map(dest => dest.LifecycleStatus, src => src.LifecycleStatus.ToString());

        TypeAdapterConfig<ServiceSpecCharacteristic, ServiceSpecCharacteristicDto>.NewConfig();

        TypeAdapterConfig<ServiceSpecCharValue, ServiceSpecCharValueDto>.NewConfig();

        TypeAdapterConfig<ServiceSpecRelationship, ServiceSpecRelationshipDto>.NewConfig()
            .Map(dest => dest.RelationshipType, src => src.RelationshipType.ToString());
    }
}
```

- [ ] **Step 6: Verify compilation**

```bash
dotnet build src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Obss.ServiceCatalog.Application.csproj 2>&1 | tail -3
```

- [ ] **Step 7: Commit**

```bash
git add src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/
git commit -m "feat(service-catalog): add application abstractions, DTOs, and mappings"
```

---

### Task 6: Application Layer — Category Commands and Queries

**Files:**
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCategory/CreateServiceCategory/CreateServiceCategoryCommand.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCategory/CreateServiceCategory/CreateServiceCategoryCommandHandler.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCategory/CreateServiceCategory/CreateServiceCategoryCommandValidator.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCategory/UpdateServiceCategory/UpdateServiceCategoryCommand.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCategory/UpdateServiceCategory/UpdateServiceCategoryCommandHandler.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCategory/UpdateServiceCategory/UpdateServiceCategoryCommandValidator.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCategory/DeleteServiceCategory/DeleteServiceCategoryCommand.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCategory/DeleteServiceCategory/DeleteServiceCategoryCommandHandler.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Queries/GetServiceCategories/GetServiceCategoriesQuery.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Queries/GetServiceCategories/GetServiceCategoriesQueryHandler.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Queries/GetServiceCategoryById/GetServiceCategoryByIdQuery.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Queries/GetServiceCategoryById/GetServiceCategoryByIdQueryHandler.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/packages.lock.json`

- [ ] **Step 1: Create CreateServiceCategory command**

```csharp
using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.CreateServiceCategory;

public sealed record CreateServiceCategoryCommand(
    string TenantId,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    DateTime? ValidFrom,
    DateTime? ValidTo
) : IRequest<Guid>;
```

- [ ] **Step 2: Create handler**

```csharp
using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Entities;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.CreateServiceCategory;

internal sealed class CreateServiceCategoryCommandHandler(IServiceCategoryRepository repository) : IRequestHandler<CreateServiceCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateServiceCategoryCommand request, CancellationToken ct)
    {
        var category = ServiceCategory.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.ParentCategoryId,
            request.ValidFrom,
            request.ValidTo);

        await repository.AddAsync(category, ct);
        return category.Id;
    }
}
```

- [ ] **Step 3: Create validator**

```csharp
using FluentValidation;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.CreateServiceCategory;

public sealed class CreateServiceCategoryCommandValidator : AbstractValidator<CreateServiceCategoryCommand>
{
    public CreateServiceCategoryCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
```

- [ ] **Step 4: Create UpdateServiceCategory command + handler + validator**

Command:
```csharp
using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.UpdateServiceCategory;

public sealed record UpdateServiceCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    DateTime? ValidFrom,
    DateTime? ValidTo
) : IRequest;
```

Handler:
```csharp
using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.UpdateServiceCategory;

internal sealed class UpdateServiceCategoryCommandHandler(IServiceCategoryRepository repository) : IRequestHandler<UpdateServiceCategoryCommand>
{
    public async Task Handle(UpdateServiceCategoryCommand request, CancellationToken ct)
    {
        var category = await repository.GetByIdAsync(request.Id, ct)
            ?? throw new ServiceCatalogDomainException($"Service category {request.Id} not found");

        category.UpdateDetails(request.Name, request.Description);
        category.SetValidityPeriod(request.ValidFrom, request.ValidTo);

        await repository.UpdateAsync(category, ct);
    }
}
```

Validator:
```csharp
using FluentValidation;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.UpdateServiceCategory;

public sealed class UpdateServiceCategoryCommandValidator : AbstractValidator<UpdateServiceCategoryCommand>
{
    public UpdateServiceCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
```

- [ ] **Step 5: Create DeleteServiceCategory command + handler**

Command:
```csharp
using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.DeleteServiceCategory;

public sealed record DeleteServiceCategoryCommand(Guid Id) : IRequest;
```

Handler:
```csharp
using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.DeleteServiceCategory;

internal sealed class DeleteServiceCategoryCommandHandler(IServiceCategoryRepository repository) : IRequestHandler<DeleteServiceCategoryCommand>
{
    public async Task Handle(DeleteServiceCategoryCommand request, CancellationToken ct)
    {
        var category = await repository.GetByIdAsync(request.Id, ct)
            ?? throw new ServiceCatalogDomainException($"Service category {request.Id} not found");

        await repository.DeleteAsync(category, ct);
    }
}
```

- [ ] **Step 6: Create GetServiceCategories query**

```csharp
using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCategories;

public sealed record GetServiceCategoriesQuery(
    string TenantId,
    Guid? ParentCategoryId = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<(List<ServiceCategoryDto> Items, int TotalCount)>;
```

Handler:
```csharp
using MediatR;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCategories;

internal sealed class GetServiceCategoriesQueryHandler(IServiceCategoryRepository repository) : IRequestHandler<GetServiceCategoriesQuery, (List<ServiceCategoryDto> Items, int TotalCount)>
{
    public async Task<(List<ServiceCategoryDto> Items, int TotalCount)> Handle(GetServiceCategoriesQuery request, CancellationToken ct)
    {
        var query = await repository.GetAllAsync(ct);
        var items = query.AsQueryable();

        if (request.ParentCategoryId.HasValue)
            items = items.Where(c => c.ParentCategoryId == request.ParentCategoryId.Value);
        else
            items = items.Where(c => c.ParentCategoryId == null);

        if (!string.IsNullOrEmpty(request.Status))
            items = items.Where(c => c.LifecycleStatus.ToString() == request.Status);

        var total = await items.CountAsync(ct);
        var result = await items
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (result.Adapt<List<ServiceCategoryDto>>(), total);
    }
}
```

- [ ] **Step 7: Create GetServiceCategoryById query + handler**

```csharp
using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCategoryById;

public sealed record GetServiceCategoryByIdQuery(Guid Id) : IRequest<ServiceCategoryDto?>;
```

```csharp
using Mapster;
using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCategoryById;

internal sealed class GetServiceCategoryByIdQueryHandler(IServiceCategoryRepository repository) : IRequestHandler<GetServiceCategoryByIdQuery, ServiceCategoryDto?>
{
    public async Task<ServiceCategoryDto?> Handle(GetServiceCategoryByIdQuery request, CancellationToken ct)
    {
        var category = await repository.GetByIdAsync(request.Id, ct);
        return category?.Adapt<ServiceCategoryDto>();
    }
}
```

- [ ] **Step 8: Create dummy packages.lock.json**

```bash
cd src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application && dotnet restore && cd ../../../..
```

- [ ] **Step 9: Verify compilation**

```bash
dotnet build src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Obss.ServiceCatalog.Application.csproj 2>&1 | tail -5
```

- [ ] **Step 10: Commit**

```bash
git add src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/
git commit -m "feat(service-catalog): add category commands and queries"
```

---

### Task 7: Application Layer — Candidate Commands and Queries

**Files:**
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCandidate/CreateServiceCandidate/CreateServiceCandidateCommand.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCandidate/CreateServiceCandidate/CreateServiceCandidateCommandHandler.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCandidate/CreateServiceCandidate/CreateServiceCandidateCommandValidator.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCandidate/UpdateServiceCandidate/UpdateServiceCandidateCommand.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCandidate/UpdateServiceCandidate/UpdateServiceCandidateCommandHandler.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCandidate/UpdateServiceCandidate/UpdateServiceCandidateCommandValidator.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCandidate/DeleteServiceCandidate/DeleteServiceCandidateCommand.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Commands/ServiceCandidate/DeleteServiceCandidate/DeleteServiceCandidateCommandHandler.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Queries/GetServiceCandidates/GetServiceCandidatesQuery.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Queries/GetServiceCandidates/GetServiceCandidatesQueryHandler.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Queries/GetServiceCandidateById/GetServiceCandidateByIdQuery.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Queries/GetServiceCandidateById/GetServiceCandidateByIdQueryHandler.cs`

- [ ] **Step 1: Create CreateServiceCandidate command**

```csharp
using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCandidate.CreateServiceCandidate;

public sealed record CreateServiceCandidateCommand(
    string TenantId,
    string Name,
    string? Description,
    Guid? ServiceSpecificationId,
    Guid? BaseCandidateId,
    string? FeatureSpecification,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    List<Guid>? CategoryIds
) : IRequest<Guid>;
```

Handler:
```csharp
using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Entities;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCandidate.CreateServiceCandidate;

internal sealed class CreateServiceCandidateCommandHandler(
    IServiceCandidateRepository candidateRepository,
    IServiceCategoryRepository categoryRepository) : IRequestHandler<CreateServiceCandidateCommand, Guid>
{
    public async Task<Guid> Handle(CreateServiceCandidateCommand request, CancellationToken ct)
    {
        var candidate = ServiceCandidate.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.ServiceSpecificationId,
            request.BaseCandidateId,
            request.FeatureSpecification,
            request.ValidFrom,
            request.ValidTo);

        if (request.CategoryIds?.Count > 0)
        {
            foreach (var catId in request.CategoryIds)
            {
                var category = await categoryRepository.GetByIdAsync(catId, ct);
                if (category != null)
                    category.AddCandidate(candidate);
            }
        }

        await candidateRepository.AddAsync(candidate, ct);
        return candidate.Id;
    }
}
```

Validator:
```csharp
using FluentValidation;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCandidate.CreateServiceCandidate;

public sealed class CreateServiceCandidateCommandValidator : AbstractValidator<CreateServiceCandidateCommand>
{
    public CreateServiceCandidateCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.FeatureSpecification).MaximumLength(4000);
    }
}
```

- [ ] **Step 2: Create remaining candidate commands following same pattern as Task 6 Steps 4-5**

- [ ] **Step 3: Create candidate queries following same pagination pattern as Task 6 Steps 6-7**

- [ ] **Step 4: Verify compilation**

```bash
dotnet build src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Obss.ServiceCatalog.Application.csproj 2>&1 | tail -5
```

- [ ] **Step 5: Commit**

```bash
git add src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/
git commit -m "feat(service-catalog): add candidate commands and queries"
```

---

### Task 8: Application Layer — Specification Commands and Queries

**Files:**
- Create: commands for Create/Update/Delete ServiceSpecification + Add/Remove Characteristic + Add/Remove CharacteristicValue + Update Characteristic + Update CharacteristicValue + Add/Remove SpecRelationship
- Create: queries for GetServiceSpecifications, GetServiceSpecificationById, GetCharacteristics, GetCharacteristicValues, GetSpecRelationships

Follow the same pattern as Tasks 6-7. 14 command files + 5 query files.

- [ ] **Step 1: Create specification commands**

- [ ] **Step 2: Create specification queries**

- [ ] **Step 3: Verify compilation**

- [ ] **Step 4: Commit**

---

### Task 9: Infrastructure Layer — Entity Configurations and DbContext

**Files:**
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Persistence/Configurations/ServiceCategoryConfiguration.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Persistence/Configurations/ServiceCandidateConfiguration.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Persistence/Configurations/ServiceSpecificationConfiguration.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Persistence/Configurations/ServiceSpecCharacteristicConfiguration.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Persistence/Configurations/ServiceSpecCharValueConfiguration.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Persistence/Configurations/ServiceSpecRelationshipConfiguration.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Persistence/ServiceCatalogDbContext.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Persistence/ServiceCatalogDbContextFactory.cs`

- [ ] **Step 1: Create ServiceCategoryConfiguration**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ServiceCatalog.Domain.Entities;

namespace Obss.ServiceCatalog.Infrastructure.Persistence.Configurations;

internal sealed class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.ToTable("service_categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ParentCategoryId);
        builder.Property(x => x.LifecycleStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.ValidFrom);
        builder.Property(x => x.ValidTo);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Ignore(x => x.IsRoot);

        builder.HasIndex(x => x.TenantId).HasDatabaseName("ix_service_categories_tenant_id");
        builder.HasIndex(x => x.ParentCategoryId).HasDatabaseName("ix_service_categories_parent_category_id");
        builder.HasIndex(x => x.LifecycleStatus).HasDatabaseName("ix_service_categories_lifecycle_status");
        builder.HasIndex(x => new { x.TenantId, x.Name }).HasDatabaseName("ix_service_categories_tenant_id_name");

        builder.HasMany(x => x.Candidates)
            .WithMany(x => x.Categories)
            .UsingEntity(j => j.ToTable("category_candidates"));
    }
}
```

- [ ] **Step 2: Create remaining configurations (same pattern)**

- [ ] **Step 3: Create ServiceCatalogDbContext**

```csharp
using Microsoft.EntityFrameworkCore;
using Obss.ServiceCatalog.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceCatalog.Infrastructure.Persistence;

public class ServiceCatalogDbContext : EfDbContext
{
    public ServiceCatalogDbContext(DbContextOptions<ServiceCatalogDbContext> options, ICurrentTenant currentTenant, IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher) { }

    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServiceCandidate> ServiceCandidates => Set<ServiceCandidate>();
    public DbSet<ServiceSpecification> ServiceSpecifications => Set<ServiceSpecification>();
    public DbSet<ServiceSpecCharacteristic> ServiceSpecCharacteristics => Set<ServiceSpecCharacteristic>();
    public DbSet<ServiceSpecCharValue> ServiceSpecCharValues => Set<ServiceSpecCharValue>();
    public DbSet<ServiceSpecRelationship> ServiceSpecRelationships => Set<ServiceSpecRelationship>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceCatalogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

- [ ] **Step 4: Create DbContextFactory**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ServiceCatalog.Infrastructure.Persistence;

public class ServiceCatalogDbContextFactory : IDesignTimeDbContextFactory<ServiceCatalogDbContext>
{
    public ServiceCatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ServiceCatalogDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=obss;Username=obss;Password=obss");
        return new ServiceCatalogDbContext(optionsBuilder.Options, new DesignTimeCurrentTenant(), new DesignTimeDomainEventDispatcher());
    }
}
```

- [ ] **Step 5: Create initial migration**

```bash
dotnet ef migrations add InitialCreate -p src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure -s src/Host/Obss.Host -c ServiceCatalogDbContext --connection "Host=localhost;Database=obss;Username=obss;Password=obss"
```

- [ ] **Step 6: Verify compilation**

```bash
dotnet build src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Obss.ServiceCatalog.Infrastructure.csproj 2>&1 | tail -5
```

- [ ] **Step 7: Commit**

```bash
git add src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/
git commit -m "feat(service-catalog): add infrastructure layer with EF Core configs and migrations"
```

---

### Task 10: Infrastructure Layer — Repositories and DI

**Files:**
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Persistence/Repositories/ServiceCategoryRepository.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Persistence/Repositories/ServiceCandidateRepository.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Persistence/Repositories/ServiceSpecificationRepository.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Create repository implementations following existing EfRepository pattern**

- [ ] **Step 2: Create DI registration**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Infrastructure.Persistence.Repositories;

namespace Obss.ServiceCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddServiceCatalogInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
        services.AddScoped<IServiceCandidateRepository, ServiceCandidateRepository>();
        services.AddScoped<IServiceSpecificationRepository, ServiceSpecificationRepository>();
        return services;
    }
}
```

- [ ] **Step 3: Commit**

---

### Task 11: API Endpoints and Module Registration

**Files:**
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Api/Endpoints/ServiceCategoryEndpoints.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Api/Endpoints/ServiceCandidateEndpoints.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Api/Endpoints/ServiceSpecificationEndpoints.cs`
- Create: `src/Modules/ServiceCatalog/Obss.ServiceCatalog.Api/Extensions/ServiceCatalogRegistration.cs`

- [ ] **Step 1: Create ServiceCategoryEndpoints**

```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ServiceCatalog.Application.Commands.ServiceCategory.CreateServiceCategory;
using Obss.ServiceCatalog.Application.Commands.ServiceCategory.DeleteServiceCategory;
using Obss.ServiceCatalog.Application.Commands.ServiceCategory.UpdateServiceCategory;
using Obss.ServiceCatalog.Application.Queries.GetServiceCategories;
using Obss.ServiceCatalog.Application.Queries.GetServiceCategoryById;

namespace Obss.ServiceCatalog.Api.Endpoints;

internal static class ServiceCategoryEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/service-categories", async (CreateServiceCategoryCommand command, ISender sender) =>
        {
            var id = await sender.Send(command);
            return Results.Created($"/api/v1/service-catalog/service-categories/{id}", id);
        });

        group.MapGet("/service-categories", async (string? status, int page, int pageSize, HttpContext context, ISender sender) =>
        {
            var tenantId = context.GetTenantId();
            var query = new GetServiceCategoriesQuery(tenantId, null, status, page, pageSize);
            var (items, total) = await sender.Send(query);
            context.Response.Headers.Append("X-Total-Count", total.ToString());
            context.Response.Headers.Append("X-Result-Count", items.Count.ToString());
            return Results.Ok(items);
        });

        group.MapGet("/service-categories/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetServiceCategoryByIdQuery(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        group.MapPatch("/service-categories/{id:guid}", async (Guid id, UpdateServiceCategoryCommand command, ISender sender) =>
        {
            if (id != command.Id) return Results.BadRequest("Id mismatch");
            await sender.Send(command);
            return Results.NoContent();
        });

        group.MapDelete("/service-categories/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteServiceCategoryCommand(id));
            return Results.NoContent();
        });
    }
}
```

- [ ] **Step 2: Create remaining endpoint files following same pattern**

- [ ] **Step 3: Create module registration**

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.ServiceCatalog.Api.Endpoints;
using Obss.ServiceCatalog.Application.Mappings;
using Obss.ServiceCatalog.Infrastructure;

namespace Obss.ServiceCatalog.Api.Extensions;

public static class ServiceCatalogRegistration
{
    public static IServiceCollection AddServiceCatalogModule(this IServiceCollection services)
    {
        services.AddServiceCatalogInfrastructure();
        ServiceCatalogMappingConfig.Configure();
        return services;
    }

    public static IEndpointRouteBuilder MapServiceCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/service-catalog").WithTags("Service Catalog");
        ServiceCategoryEndpoints.Map(group);
        ServiceCandidateEndpoints.Map(group);
        ServiceSpecificationEndpoints.Map(group);
        return app;
    }
}
```

- [ ] **Step 4: Commit**

---

### Task 12: Host Registration

**Files:**
- Modify: `src/Host/Obss.Host/Modules/ModuleLoader.cs`
- Modify: `src/Host/Obss.Host/Program.cs`

- [ ] **Step 1: Register in ModuleLoader**

Add to `ModuleLoader.cs`:
```csharp
using Obss.ServiceCatalog.Api.Extensions;
```

In `RegisterModules`:
```csharp
services.AddServiceCatalogModule();
```

- [ ] **Step 2: Map endpoints in Program.cs**

```csharp
app.MapServiceCatalogEndpoints();
```

- [ ] **Step 3: Verify build**

```bash
dotnet build src/Host/Obss.Host/Obss.Host.csproj 2>&1 | tail -5
```

- [ ] **Step 4: Commit**

---

### Task 13: Integration — ProductCatalog, ServiceInventory, Provisioning

- [ ] **Step 1: Add optional ServiceSpecificationId to ProductSpecification**

In ProductCatalog Domain entity `ProductSpecification.cs`, add:
```csharp
public Guid? ServiceSpecificationId { get; private set; }
```

- [ ] **Step 2: Add optional ServiceSpecificationId to ServiceInventory Service entity**

In ServiceInventory Domain entity `Service.cs`, replace free-text `ServiceType` with:
```csharp
public Guid? ServiceSpecificationId { get; private set; }
```

- [ ] **Step 3: Add optional ServiceSpecificationId to ProvisioningTemplate**

In Provisioning Domain entity `ProvisioningTemplate.cs`, replace string `ServiceType` with:
```csharp
public Guid? ServiceSpecificationId { get; private set; }
```

- [ ] **Step 4: Update EF configurations for new columns**

- [ ] **Step 5: Generate migrations for affected modules**

```bash
dotnet ef migrations add AddServiceSpecificationId -p src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure -s src/Host/Obss.Host -c CatalogDbContext
```

- [ ] **Step 6: Verify build**

```bash
dotnet build Obss.sln 2>&1 | tail -5
```

- [ ] **Step 7: Commit**

---

### Task 14: Frontend — Hooks

**Files:**
- Create: `frontend/src/api/hooks/use-service-catalog.ts`

Following existing hook patterns from `use-products.ts`, `use-offers.ts`:

- [ ] **Step 1: Create React Query hooks for all Service Catalog resources**

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/services/api';
import type {
  ServiceCategoryDto,
  ServiceCandidateDto,
  ServiceSpecificationDto,
  ServiceSpecCharacteristicDto,
  ServiceSpecCharValueDto,
  ServiceSpecRelationshipDto,
} from '@/api/generated/dto';

// Categories
export function useServiceCategories(params?: { status?: string; page?: number; pageSize?: number }) {
  return useQuery({
    queryKey: ['service-categories', params],
    queryFn: () => api.get('/api/v1/service-catalog/service-categories', { params }).then(r => r.data),
  });
}

export function useServiceCategory(id: string) {
  return useQuery({
    queryKey: ['service-category', id],
    queryFn: () => api.get(`/api/v1/service-catalog/service-categories/${id}`).then(r => r.data),
    enabled: !!id,
  });
}

export function useCreateServiceCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: any) => api.post('/api/v1/service-catalog/service-categories', data).then(r => r.data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['service-categories'] }),
  });
}

// Candidates
export function useServiceCandidates(params?: { status?: string; page?: number; pageSize?: number }) { /* ... */ }
export function useServiceCandidate(id: string) { /* ... */ }
export function useCreateServiceCandidate() { /* ... */ }

// Specifications
export function useServiceSpecifications(params?: { status?: string; page?: number; pageSize?: number }) { /* ... */ }
export function useServiceSpecification(id: string) { /* ... */ }
export function useCreateServiceSpecification() { /* ... */ }
export function useServiceCharacteristics(specId: string) { /* ... */ }
export function useServiceCharacteristicValues(charId: string) { /* ... */ }
```

- [ ] **Step 2: Commit**

---

### Task 15: Frontend — Pages

**Files:**
- Create: page files under `frontend/src/app/service-catalog/`

- [ ] **Step 1: Create main service-catalog page with 3 tabs**

- [ ] **Step 2: Create categories list and detail pages**

- [ ] **Step 3: Create candidates list, detail, and create pages**

- [ ] **Step 4: Create specifications list, detail, and edit pages**

- [ ] **Step 5: Commit**

---

### Task 16: Verification

- [ ] **Step 1: Full solution build**

```bash
dotnet build Obss.sln 2>&1 | tail -10
```
Expected: Build succeeded, 0 warnings

- [ ] **Step 2: Run tests**

```bash
dotnet test --no-build --configuration Release 2>&1 | tail -10
```
Expected: All tests pass

- [ ] **Step 3: Create and apply initial migration**

```bash
dotnet ef database update -p src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure -s src/Host/Obss.Host -c ServiceCatalogDbContext --connection "..."
```
