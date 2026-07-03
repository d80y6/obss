# TMF620 Product Catalog Alignment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete TMF620 v5.0.1 alignment for the ProductCatalog module — ProductSpecification (full entity), ProductOfferingTerm, BundledProductOffering, and pricing enhancements.

**Architecture:** Four additive sub-projects executed sequentially. Each follows the existing module patterns: Domain entities (AggregateRoot/Entity/ValueObject base classes) → EF Core configs → DTOs/Mapster → MediatR commands/queries → Minimal API endpoints → frontend types. All changes to `obss_catalog` database, non-breaking.

**Tech Stack:** .NET 9, EF Core/Npgsql, MediatR, FluentValidation, Mapster, Next.js 16, React Query, zod.

---

## Phase 1: ProductSpecification Entity

### Task 1.1: Domain Entities, Enums, Events

**Files:**
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Enums/SpecificationRelationshipType.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/ProductSpecification.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/ProductSpecificationCharacteristic.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/ProductSpecificationCharacteristicValue.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/ProductSpecificationRelationship.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Events/ProductSpecificationCreatedDomainEvent.cs`

- [ ] **Create `SpecificationRelationshipType` enum**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Enums/SpecificationRelationshipType.cs`

```csharp
namespace Obss.ProductCatalog.Domain.Domain.ValueObjects;

public enum SpecificationRelationshipType
{
    Dependency = 1,
    Substitution = 2,
    Exclusion = 3,
    Optional = 4
}
```

- [ ] **Create `ProductSpecification` entity**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/ProductSpecification.cs`

```csharp
using Obss.ProductCatalog.Domain.Domain.Events;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class ProductSpecification : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<ProductSpecificationCharacteristic> _characteristics = [];
    private readonly List<ProductSpecificationRelationship> _relationships = [];

    private ProductSpecification() { }

    private ProductSpecification(
        Guid id,
        string tenantId,
        string name,
        string? description,
        string? brand,
        string? version,
        string? productNumber)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        Brand = brand;
        Version = version;
        ProductNumber = productNumber;
        LifecycleStatus = LifecycleStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductSpecificationCreatedDomainEvent(id, tenantId, name));
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Brand { get; private set; }
    public string? Version { get; private set; }
    public string? ProductNumber { get; private set; }
    public LifecycleStatus LifecycleStatus { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<ProductSpecificationCharacteristic> Characteristics => _characteristics.AsReadOnly();
    public IReadOnlyCollection<ProductSpecificationRelationship> Relationships => _relationships.AsReadOnly();

    public static ProductSpecification Create(
        string tenantId,
        string name,
        string? description,
        string? brand,
        string? version,
        string? productNumber)
    {
        return new ProductSpecification(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            brand,
            version,
            productNumber);
    }

    public void Activate()
    {
        if (LifecycleStatus == LifecycleStatus.Active) return;
        LifecycleStatus = LifecycleStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Retire()
    {
        if (LifecycleStatus == LifecycleStatus.Retired) return;
        LifecycleStatus = LifecycleStatus.Retired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Discontinue()
    {
        if (LifecycleStatus == LifecycleStatus.Discontinued) return;
        LifecycleStatus = LifecycleStatus.Discontinued;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description, string? brand, string? version, string? productNumber)
    {
        Name = name;
        Description = description;
        Brand = brand;
        Version = version;
        ProductNumber = productNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetValidityPeriod(DateTime? validFrom, DateTime? validTo)
    {
        ValidFrom = validFrom;
        ValidTo = validTo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCharacteristic(ProductSpecificationCharacteristic characteristic)
    {
        _characteristics.Add(characteristic);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveCharacteristic(ProductSpecificationCharacteristic characteristic)
    {
        _characteristics.Remove(characteristic);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRelationship(ProductSpecificationRelationship relationship)
    {
        _relationships.Add(relationship);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRelationship(ProductSpecificationRelationship relationship)
    {
        _relationships.Remove(relationship);
        UpdatedAt = DateTime.UtcNow;
    }
}
```

- [ ] **Create `ProductSpecificationCharacteristic` entity**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/ProductSpecificationCharacteristic.cs`

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class ProductSpecificationCharacteristic : Entity<Guid>
{
    private readonly List<ProductSpecificationCharacteristicValue> _values = [];

    private ProductSpecificationCharacteristic() { }

    public ProductSpecificationCharacteristic(
        Guid id,
        Guid productSpecificationId,
        string name,
        string? description,
        string valueType,
        bool configurable,
        decimal? minValue,
        decimal? maxValue,
        string? regex,
        int sortOrder,
        int? maxCardinality,
        bool isRequired)
        : base(id)
    {
        ProductSpecificationId = productSpecificationId;
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

    public Guid ProductSpecificationId { get; private set; }
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

    public IReadOnlyCollection<ProductSpecificationCharacteristicValue> Values => _values.AsReadOnly();

    public void UpdateDetails(
        string name,
        string? description,
        string valueType,
        bool configurable,
        decimal? minValue,
        decimal? maxValue,
        string? regex,
        int sortOrder,
        int? maxCardinality,
        bool isRequired)
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

    public void AddValue(ProductSpecificationCharacteristicValue value)
    {
        _values.Add(value);
    }

    public void RemoveValue(ProductSpecificationCharacteristicValue value)
    {
        _values.Remove(value);
    }
}
```

- [ ] **Create `ProductSpecificationCharacteristicValue` entity**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/ProductSpecificationCharacteristicValue.cs`

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class ProductSpecificationCharacteristicValue : Entity<Guid>
{
    private ProductSpecificationCharacteristicValue() { }

    public ProductSpecificationCharacteristicValue(
        Guid id,
        Guid characteristicId,
        string value,
        string? unitOfMeasure,
        bool isDefault,
        decimal? valueFrom,
        decimal? valueTo,
        string? rangeInterval,
        DateTime? validFrom,
        DateTime? validTo)
        : base(id)
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

    public Guid CharacteristicId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public string? UnitOfMeasure { get; private set; }
    public bool IsDefault { get; private set; }
    public decimal? ValueFrom { get; private set; }
    public decimal? ValueTo { get; private set; }
    public string? RangeInterval { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    public void Update(string value, string? unitOfMeasure, bool isDefault, decimal? valueFrom, decimal? valueTo, string? rangeInterval, DateTime? validFrom, DateTime? validTo)
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

- [ ] **Create `ProductSpecificationRelationship` entity**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/ProductSpecificationRelationship.cs`

```csharp
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class ProductSpecificationRelationship : Entity<Guid>
{
    private ProductSpecificationRelationship() { }

    public ProductSpecificationRelationship(
        Guid id,
        Guid productSpecificationId,
        Guid targetSpecificationId,
        SpecificationRelationshipType relationshipType,
        string? role,
        DateTime? validFrom,
        DateTime? validTo)
        : base(id)
    {
        ProductSpecificationId = productSpecificationId;
        TargetSpecificationId = targetSpecificationId;
        RelationshipType = relationshipType;
        Role = role;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public Guid ProductSpecificationId { get; private set; }
    public Guid TargetSpecificationId { get; private set; }
    public SpecificationRelationshipType RelationshipType { get; private set; }
    public string? Role { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
}
```

- [ ] **Create `ProductSpecificationCreatedDomainEvent`**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Events/ProductSpecificationCreatedDomainEvent.cs`

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Events;

public sealed class ProductSpecificationCreatedDomainEvent : DomainEvent
{
    public ProductSpecificationCreatedDomainEvent(Guid productSpecificationId, string tenantId, string name)
    {
        ProductSpecificationId = productSpecificationId;
        TenantId = tenantId;
        Name = name;
    }

    public Guid ProductSpecificationId { get; }
    public string TenantId { get; }
    public string Name { get; }
}
```

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/
git commit -m "feat(catalog): add ProductSpecification domain entities, enums, and events"
```

---

### Task 1.2: Update Product Entity with ProductNumber and ProductSpecificationId

**Files:**
- Modify: `src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/Product.cs`

- [ ] **Add ProductNumber and ProductSpecificationId to Product**

- Add property `ProductNumber: string?` after the existing `TaxCategory` property
- Add property `ProductSpecificationId: Guid?` after `ProductNumber`
- Add navigation property `ProductSpecification: ProductSpecification?` (private set)
- Update constructor and `Create()` factory to accept optional `productNumber` parameter
- Add method `public void AssignProductSpecification(Guid productSpecificationId)` to set the reference

Add after `TaxCategory`:
```csharp
public string? ProductNumber { get; private set; }
public Guid? ProductSpecificationId { get; private set; }
public ProductSpecification? ProductSpecification { get; private set; }
```

Update constructor to accept `string? productNumber`:
```csharp
private Product(
    Guid id,
    string tenantId,
    string name,
    string? description,
    Guid? categoryId,
    ProductType productType,
    bool isShippable,
    bool taxable,
    string? taxCategory,
    string? productNumber)
    : base(id)
{
    // ...existing code...
    ProductNumber = productNumber;
    // ...
}
```

Update `Create()` factory to accept and pass `productNumber`:
```csharp
public static Product Create(
    string tenantId,
    string name,
    string? description,
    Guid? categoryId,
    ProductType productType,
    bool isShippable,
    bool taxable,
    string? taxCategory,
    string? productNumber = null)
{
    return new Product(
        Guid.NewGuid(),
        tenantId, name, description, categoryId,
        productType, isShippable, taxable, taxCategory, productNumber);
}
```

Add method:
```csharp
public void AssignProductSpecification(Guid productSpecificationId)
{
    ProductSpecificationId = productSpecificationId;
    UpdatedAt = DateTime.UtcNow;
}
```

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Domain/Entities/Product.cs
git commit -m "feat(catalog): add ProductNumber and ProductSpecificationId to Product entity"
```

---

### Task 1.3: EF Configurations for ProductSpecification Entities

**Files:**
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Configurations/ProductSpecificationConfiguration.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Configurations/ProductSpecificationCharacteristicConfiguration.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Configurations/ProductSpecificationCharacteristicValueConfiguration.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Configurations/ProductSpecificationRelationshipConfiguration.cs`
- Modify: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`
- Modify: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/CatalogDbContext.cs`

- [ ] **Create `ProductSpecificationConfiguration`**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Configurations/ProductSpecificationConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
{
    public void Configure(EntityTypeBuilder<ProductSpecification> builder)
    {
        builder.ToTable("product_specifications");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.Id).ValueGeneratedNever();

        builder.Property(ps => ps.TenantId)
            .HasColumnName("tenant_id").HasMaxLength(100).IsRequired();

        builder.Property(ps => ps.Name)
            .HasColumnName("name").HasMaxLength(200).IsRequired();

        builder.Property(ps => ps.Description)
            .HasColumnName("description").HasMaxLength(2000);

        builder.Property(ps => ps.Brand)
            .HasColumnName("brand").HasMaxLength(200);

        builder.Property(ps => ps.Version)
            .HasColumnName("version").HasMaxLength(100);

        builder.Property(ps => ps.ProductNumber)
            .HasColumnName("product_number").HasMaxLength(100);

        builder.Property(ps => ps.LifecycleStatus)
            .HasColumnName("lifecycle_status")
            .HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(ps => ps.ValidFrom).HasColumnName("valid_from");
        builder.Property(ps => ps.ValidTo).HasColumnName("valid_to");
        builder.Property(ps => ps.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(ps => ps.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasMany(ps => ps.Characteristics)
            .WithOne()
            .HasForeignKey(c => c.ProductSpecificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ps => ps.Relationships)
            .WithOne()
            .HasForeignKey(r => r.ProductSpecificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ps => ps.TenantId).HasDatabaseName("ix_product_specifications_tenant_id");
        builder.HasIndex(ps => ps.Name).HasDatabaseName("ix_product_specifications_name");
        builder.HasIndex(ps => ps.LifecycleStatus).HasDatabaseName("ix_product_specifications_lifecycle_status");
        builder.HasIndex(ps => new { ps.TenantId, ps.ProductNumber })
            .HasDatabaseName("ix_product_specifications_tenant_id_product_number")
            .IsUnique()
            .HasFilter("\"product_number\" IS NOT NULL");
    }
}
```

- [ ] **Create `ProductSpecificationCharacteristicConfiguration`**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Configurations/ProductSpecificationCharacteristicConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductSpecificationCharacteristicConfiguration : IEntityTypeConfiguration<ProductSpecificationCharacteristic>
{
    public void Configure(EntityTypeBuilder<ProductSpecificationCharacteristic> builder)
    {
        builder.ToTable("product_specification_characteristics");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.ProductSpecificationId).HasColumnName("product_specification_id").IsRequired();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(c => c.ValueType).HasColumnName("value_type").HasMaxLength(50).IsRequired();
        builder.Property(c => c.Configurable).HasColumnName("configurable").IsRequired();
        builder.Property(c => c.MinValue).HasColumnName("min_value").HasColumnType("decimal(18,4)");
        builder.Property(c => c.MaxValue).HasColumnName("max_value").HasColumnType("decimal(18,4)");
        builder.Property(c => c.Regex).HasColumnName("regex").HasMaxLength(500);
        builder.Property(c => c.SortOrder).HasColumnName("sort_order").IsRequired();
        builder.Property(c => c.MaxCardinality).HasColumnName("max_cardinality");
        builder.Property(c => c.IsRequired).HasColumnName("is_required").IsRequired();

        builder.HasMany(c => c.Values)
            .WithOne()
            .HasForeignKey(v => v.CharacteristicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ProductSpecificationId).HasDatabaseName("ix_spec_characteristics_product_specification_id");
    }
}
```

- [ ] **Create `ProductSpecificationCharacteristicValueConfiguration`**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Configurations/ProductSpecificationCharacteristicValueConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductSpecificationCharacteristicValueConfiguration : IEntityTypeConfiguration<ProductSpecificationCharacteristicValue>
{
    public void Configure(EntityTypeBuilder<ProductSpecificationCharacteristicValue> builder)
    {
        builder.ToTable("product_specification_characteristic_values");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.CharacteristicId).HasColumnName("characteristic_id").IsRequired();
        builder.Property(v => v.Value).HasColumnName("value").HasMaxLength(1000).IsRequired();
        builder.Property(v => v.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(50);
        builder.Property(v => v.IsDefault).HasColumnName("is_default").IsRequired();
        builder.Property(v => v.ValueFrom).HasColumnName("value_from").HasColumnType("decimal(18,4)");
        builder.Property(v => v.ValueTo).HasColumnName("value_to").HasColumnType("decimal(18,4)");
        builder.Property(v => v.RangeInterval).HasColumnName("range_interval").HasMaxLength(50);
        builder.Property(v => v.ValidFrom).HasColumnName("valid_from");
        builder.Property(v => v.ValidTo).HasColumnName("valid_to");

        builder.HasIndex(v => v.CharacteristicId).HasDatabaseName("ix_spec_char_values_characteristic_id");
    }
}
```

- [ ] **Create `ProductSpecificationRelationshipConfiguration`**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Configurations/ProductSpecificationRelationshipConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.ProductCatalog.Domain.Domain.Entities;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductSpecificationRelationshipConfiguration : IEntityTypeConfiguration<ProductSpecificationRelationship>
{
    public void Configure(EntityTypeBuilder<ProductSpecificationRelationship> builder)
    {
        builder.ToTable("product_specification_relationships");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.ProductSpecificationId).HasColumnName("product_specification_id").IsRequired();
        builder.Property(r => r.TargetSpecificationId).HasColumnName("target_specification_id").IsRequired();

        builder.Property(r => r.RelationshipType)
            .HasColumnName("relationship_type")
            .HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(r => r.Role).HasColumnName("role").HasMaxLength(100);
        builder.Property(r => r.ValidFrom).HasColumnName("valid_from");
        builder.Property(r => r.ValidTo).HasColumnName("valid_to");

        builder.HasIndex(r => r.ProductSpecificationId).HasDatabaseName("ix_spec_relationships_product_specification_id");
        builder.HasIndex(r => r.TargetSpecificationId).HasDatabaseName("ix_spec_relationships_target_specification_id");
    }
}
```

- [ ] **Add ProductNumber and ProductSpecificationId columns to Product config**

Add to `ProductConfiguration.Configure()` after the `TaxCategory` property block:

```csharp
builder.Property(p => p.ProductNumber)
    .HasColumnName("product_number")
    .HasMaxLength(100);

builder.Property(p => p.ProductSpecificationId)
    .HasColumnName("product_specification_id");

builder.HasOne(p => p.ProductSpecification)
    .WithMany()
    .HasForeignKey(p => p.ProductSpecificationId)
    .OnDelete(DeleteBehavior.SetNull);
```

- [ ] **Add DbSet for ProductSpecification in CatalogDbContext**

Add to `CatalogDbContext`:

```csharp
public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
```

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/
git commit -m "feat(catalog): add EF configurations for ProductSpecification entities"
```

---

### Task 1.4: Repository Interface + Implementation

**Files:**
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Abstractions/IProductSpecificationRepository.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Repositories/ProductSpecificationRepository.cs`

- [ ] **Create `IProductSpecificationRepository`**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Abstractions/IProductSpecificationRepository.cs`

```csharp
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ProductCatalog.Application.Abstractions;

public interface IProductSpecificationRepository : IRepository<ProductSpecification>
{
    Task<ProductSpecification?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ProductSpecification> Items, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        LifecycleStatus? status,
        string? brand,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
```

- [ ] **Create `ProductSpecificationRepository`**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Repositories/ProductSpecificationRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Infrastructure.Persistence.Repositories;

public sealed class ProductSpecificationRepository : EfRepository<ProductSpecification>, IProductSpecificationRepository
{
    public ProductSpecificationRepository(CatalogDbContext context)
        : base(context)
    {
    }

    public async Task<ProductSpecification?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ps => ps.Characteristics)
                .ThenInclude(c => c.Values)
            .Include(ps => ps.Relationships)
            .FirstOrDefaultAsync(ps => ps.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<ProductSpecification> Items, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        LifecycleStatus? status,
        string? brand,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(ps => ps.Characteristics)
                .ThenInclude(c => c.Values)
            .Include(ps => ps.Relationships)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(ps => ps.LifecycleStatus == status.Value);

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(ps => ps.Brand != null && ps.Brand.Contains(brand));

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(ps =>
                ps.Name.Contains(searchTerm) ||
                (ps.Description != null && ps.Description.Contains(searchTerm)));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(ps => ps.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
```

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Abstractions/IProductSpecificationRepository.cs src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Repositories/ProductSpecificationRepository.cs
git commit -m "feat(catalog): add ProductSpecification repository"
```

---

### Task 1.5: DTOs

**Files:**
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/DTOs/ProductSpecificationDto.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/DTOs/ProductSpecificationCharacteristicDto.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/DTOs/ProductSpecificationCharacteristicValueDto.cs`
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/DTOs/ProductSpecificationRelationshipDto.cs`

- [ ] **Create DTOs**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/DTOs/ProductSpecificationDto.cs`

```csharp
using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Application.DTOs;

public sealed record ProductSpecificationDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string? Brand,
    string? Version,
    string? ProductNumber,
    LifecycleStatus LifecycleStatus,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ProductSpecificationCharacteristicDto> Characteristics,
    List<ProductSpecificationRelationshipDto> Relationships);

public sealed record ProductSpecificationCharacteristicDto(
    Guid Id,
    Guid ProductSpecificationId,
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
    List<ProductSpecificationCharacteristicValueDto> Values);

public sealed record ProductSpecificationCharacteristicValueDto(
    Guid Id,
    Guid CharacteristicId,
    string Value,
    string? UnitOfMeasure,
    bool IsDefault,
    decimal? ValueFrom,
    decimal? ValueTo,
    string? RangeInterval,
    DateTime? ValidFrom,
    DateTime? ValidTo);

public sealed record ProductSpecificationRelationshipDto(
    Guid Id,
    Guid ProductSpecificationId,
    Guid TargetSpecificationId,
    SpecificationRelationshipType RelationshipType,
    string? Role,
    DateTime? ValidFrom,
    DateTime? ValidTo);
```

- [ ] **Update `ProductDto` to include new fields**

Add to existing `ProductDto` record:
```csharp
string? ProductNumber,
Guid? ProductSpecificationId,
ProductSpecificationDto? ProductSpecification,
```

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Application/DTOs/
git commit -m "feat(catalog): add ProductSpecification DTOs"
```

---

### Task 1.6: Mapster Mappings

**Files:**
- Modify: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Mappings/CatalogMappingConfig.cs`

- [ ] **Add ProductSpecification mappings**

Add to `CatalogMappingConfig.Configure()`:

```csharp
TypeAdapterConfig<ProductSpecification, ProductSpecificationDto>.NewConfig()
    .Map(dest => dest.Id, src => src.Id)
    .Map(dest => dest.Characteristics, src => src.Characteristics.Adapt<List<ProductSpecificationCharacteristicDto>>())
    .Map(dest => dest.Relationships, src => src.Relationships.Adapt<List<ProductSpecificationRelationshipDto>>());

TypeAdapterConfig<ProductSpecificationCharacteristic, ProductSpecificationCharacteristicDto>.NewConfig()
    .Map(dest => dest.Id, src => src.Id)
    .Map(dest => dest.Values, src => src.Values.Adapt<List<ProductSpecificationCharacteristicValueDto>>());

TypeAdapterConfig<ProductSpecificationCharacteristicValue, ProductSpecificationCharacteristicValueDto>.NewConfig()
    .Map(dest => dest.Id, src => src.Id);

TypeAdapterConfig<ProductSpecificationRelationship, ProductSpecificationRelationshipDto>.NewConfig()
    .Map(dest => dest.Id, src => src.Id);

// Update Product -> ProductDto mapping to include new fields
TypeAdapterConfig<Product, ProductDto>.NewConfig()
    .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null)
    .Map(dest => dest.Specifications, src => src.Specifications.Adapt<List<ProductSpecificationDto>>())
    .Map(dest => dest.Offers, src => src.ProductOffers.Select(po => po.Offer).Adapt<List<OfferDto>>())
    .Map(dest => dest.Id, src => src.Id)
    .Map(dest => dest.ProductNumber, src => src.ProductNumber)
    .Map(dest => dest.ProductSpecificationId, src => src.ProductSpecificationId);
```

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Mappings/CatalogMappingConfig.cs
git commit -m "feat(catalog): add Mapster mappings for ProductSpecification"
```

---

### Task 1.7: Commands for ProductSpecification CRUD

Create 12 command + handler + validator files following the existing pattern. Full code for Create as example; others follow identical structure with different types.

**Files to create:**
- `.../Commands/CreateProductSpecification/CreateProductSpecificationCommand.cs`
- `.../Commands/CreateProductSpecification/CreateProductSpecificationCommandHandler.cs`
- `.../Commands/CreateProductSpecification/CreateProductSpecificationCommandValidator.cs`
- `.../Commands/UpdateProductSpecification/UpdateProductSpecificationCommand.cs`
- `.../Commands/UpdateProductSpecification/UpdateProductSpecificationCommandHandler.cs`
- `.../Commands/UpdateProductSpecification/UpdateProductSpecificationCommandValidator.cs`
- `.../Commands/PatchProductSpecification/PatchProductSpecificationCommand.cs`
- `.../Commands/PatchProductSpecification/PatchProductSpecificationCommandHandler.cs`
- `.../Commands/DeleteProductSpecification/DeleteProductSpecificationCommand.cs`
- `.../Commands/DeleteProductSpecification/DeleteProductSpecificationCommandHandler.cs`
- `.../Commands/AddCharacteristic/AddCharacteristicCommand.cs`
- `.../Commands/AddCharacteristic/AddCharacteristicCommandHandler.cs`
- `.../Commands/AddCharacteristic/AddCharacteristicCommandValidator.cs`
- `.../Commands/UpdateCharacteristic/UpdateCharacteristicCommand.cs`
- `.../Commands/UpdateCharacteristic/UpdateCharacteristicCommandHandler.cs`
- `.../Commands/UpdateCharacteristic/UpdateCharacteristicCommandValidator.cs`
- `.../Commands/RemoveCharacteristic/RemoveCharacteristicCommand.cs`
- `.../Commands/RemoveCharacteristic/RemoveCharacteristicCommandHandler.cs`
- `.../Commands/AddCharacteristicValue/AddCharacteristicValueCommand.cs`
- `.../Commands/AddCharacteristicValue/AddCharacteristicValueCommandHandler.cs`
- `.../Commands/AddCharacteristicValue/AddCharacteristicValueCommandValidator.cs`
- `.../Commands/UpdateCharacteristicValue/UpdateCharacteristicValueCommand.cs`
- `.../Commands/UpdateCharacteristicValue/UpdateCharacteristicValueCommandHandler.cs`
- `.../Commands/UpdateCharacteristicValue/UpdateCharacteristicValueCommandValidator.cs`
- `.../Commands/RemoveCharacteristicValue/RemoveCharacteristicValueCommand.cs`
- `.../Commands/RemoveCharacteristicValue/RemoveCharacteristicValueCommandHandler.cs`
- `.../Commands/AddSpecificationRelationship/AddSpecificationRelationshipCommand.cs`
- `.../Commands/AddSpecificationRelationship/AddSpecificationRelationshipCommandHandler.cs`
- `.../Commands/AddSpecificationRelationship/AddSpecificationRelationshipCommandValidator.cs`
- `.../Commands/RemoveSpecificationRelationship/RemoveSpecificationRelationshipCommand.cs`
- `.../Commands/RemoveSpecificationRelationship/RemoveSpecificationRelationshipCommandHandler.cs`

All base path: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Commands/`

- [ ] **Create CreateProductSpecification command + handler + validator** (example pattern)

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Commands/CreateProductSpecification/CreateProductSpecificationCommand.cs`

```csharp
using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateProductSpecification;

public sealed record CreateProductSpecificationCommand(
    string TenantId,
    string Name,
    string? Description,
    string? Brand,
    string? Version,
    string? ProductNumber,
    List<CreateCharacteristicItem>? Characteristics,
    List<CreateRelationshipItem>? Relationships) : IRequest<Result<ProductSpecificationDto>>;

public sealed record CreateCharacteristicItem(
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
    List<CreateCharacteristicValueItem>? Values);

public sealed record CreateCharacteristicValueItem(
    string Value,
    string? UnitOfMeasure,
    bool IsDefault,
    decimal? ValueFrom,
    decimal? ValueTo,
    string? RangeInterval,
    DateTime? ValidFrom,
    DateTime? ValidTo);

public sealed record CreateRelationshipItem(
    Guid TargetSpecificationId,
    SpecificationRelationshipType RelationshipType,
    string? Role,
    DateTime? ValidFrom,
    DateTime? ValidTo);
```

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Commands/CreateProductSpecification/CreateProductSpecificationCommandHandler.cs`

```csharp
using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateProductSpecification;

public sealed class CreateProductSpecificationCommandHandler : IRequestHandler<CreateProductSpecificationCommand, Result<ProductSpecificationDto>>
{
    private readonly IProductSpecificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductSpecificationCommandHandler(IProductSpecificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductSpecificationDto>> Handle(CreateProductSpecificationCommand request, CancellationToken cancellationToken)
    {
        var spec = ProductSpecification.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.Brand,
            request.Version,
            request.ProductNumber);

        if (request.Characteristics is not null)
        {
            foreach (var charItem in request.Characteristics)
            {
                var characteristic = new ProductSpecificationCharacteristic(
                    Guid.NewGuid(),
                    spec.Id,
                    charItem.Name,
                    charItem.Description,
                    charItem.ValueType,
                    charItem.Configurable,
                    charItem.MinValue,
                    charItem.MaxValue,
                    charItem.Regex,
                    charItem.SortOrder,
                    charItem.MaxCardinality,
                    charItem.IsRequired);

                if (charItem.Values is not null)
                {
                    foreach (var valItem in charItem.Values)
                    {
                        var value = new ProductSpecificationCharacteristicValue(
                            Guid.NewGuid(),
                            characteristic.Id,
                            valItem.Value,
                            valItem.UnitOfMeasure,
                            valItem.IsDefault,
                            valItem.ValueFrom,
                            valItem.ValueTo,
                            valItem.RangeInterval,
                            valItem.ValidFrom,
                            valItem.ValidTo);
                        characteristic.AddValue(value);
                    }
                }

                spec.AddCharacteristic(characteristic);
            }
        }

        if (request.Relationships is not null)
        {
            foreach (var relItem in request.Relationships)
            {
                var relationship = new ProductSpecificationRelationship(
                    Guid.NewGuid(),
                    spec.Id,
                    relItem.TargetSpecificationId,
                    relItem.RelationshipType,
                    relItem.Role,
                    relItem.ValidFrom,
                    relItem.ValidTo);
                spec.AddRelationship(relationship);
            }
        }

        await _repository.AddAsync(spec, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(spec.Adapt<ProductSpecificationDto>());
    }
}
```

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Commands/CreateProductSpecification/CreateProductSpecificationCommandValidator.cs`

```csharp
using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.CreateProductSpecification;

public sealed class CreateProductSpecificationCommandValidator : AbstractValidator<CreateProductSpecificationCommand>
{
    public CreateProductSpecificationCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.Brand).MaximumLength(200).When(x => x.Brand is not null);
        RuleFor(x => x.Version).MaximumLength(100).When(x => x.Version is not null);
        RuleFor(x => x.ProductNumber).MaximumLength(100).When(x => x.ProductNumber is not null);
    }
}
```

- [ ] **Create remaining ProductSpecification commands**

All follow the same pattern. Key differences:
- `UpdateProductSpecificationCommand` — takes `ProductSpecificationId`, calls `spec.UpdateDetails()`
- `PatchProductSpecificationCommand` — takes `ProductSpecificationId` + nullable fields, applies non-null ones
- `DeleteProductSpecificationCommand` — takes `ProductSpecificationId`, calls `_repository.DeleteAsync()`
- `AddCharacteristicCommand` — takes `ProductSpecificationId` + characteristic data, creates and adds to spec
- `UpdateCharacteristicCommand` — takes characteristic IDs + data, calls `characteristic.UpdateDetails()`
- `RemoveCharacteristicCommand` — takes characteristic ID, calls `spec.RemoveCharacteristic()`
- `AddCharacteristicValueCommand` — takes characteristic ID + value data
- `UpdateCharacteristicValueCommand` — takes value ID + data
- `RemoveCharacteristicValueCommand` — takes value ID
- `AddSpecificationRelationshipCommand` — takes spec ID + relationship data
- `RemoveSpecificationRelationshipCommand` — takes relationship ID

Each handler: fetch spec → validate → mutate → save → return DTO.

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Commands/
git commit -m "feat(catalog): add ProductSpecification CRUD commands"
```

---

### Task 1.8: Queries

**Files:**
- Create: `.../Queries/GetProductSpecifications/GetProductSpecificationsQuery.cs`
- Create: `.../Queries/GetProductSpecifications/GetProductSpecificationsQueryHandler.cs`
- Create: `.../Queries/GetProductSpecificationById/GetProductSpecificationByIdQuery.cs`
- Create: `.../Queries/GetProductSpecificationById/GetProductSpecificationByIdQueryHandler.cs`

Base path: `src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Queries/`

- [ ] **Create queries + handlers**

`GetProductSpecificationsQuery.cs`:
```csharp
using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductSpecifications;

public sealed record GetProductSpecificationsQuery(
    string? SearchTerm,
    LifecycleStatus? Status,
    string? Brand,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedResult<ProductSpecificationDto>>>;
```

`GetProductSpecificationsQueryHandler.cs`:
```csharp
using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductSpecifications;

public sealed class GetProductSpecificationsQueryHandler : IRequestHandler<GetProductSpecificationsQuery, Result<PaginatedResult<ProductSpecificationDto>>>
{
    private readonly IProductSpecificationRepository _repository;

    public GetProductSpecificationsQueryHandler(IProductSpecificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedResult<ProductSpecificationDto>>> Handle(GetProductSpecificationsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetFilteredAsync(
            request.SearchTerm,
            request.Status,
            request.Brand,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(new PaginatedResult<ProductSpecificationDto>(
            items.Adapt<List<ProductSpecificationDto>>(),
            totalCount,
            request.Page,
            request.PageSize));
    }
}
```

`GetProductSpecificationByIdQuery.cs`:
```csharp
using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductSpecificationById;

public sealed record GetProductSpecificationByIdQuery(Guid Id) : IRequest<Result<ProductSpecificationDto>>;
```

`GetProductSpecificationByIdQueryHandler.cs`:
```csharp
using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductSpecificationById;

public sealed class GetProductSpecificationByIdQueryHandler : IRequestHandler<GetProductSpecificationByIdQuery, Result<ProductSpecificationDto>>
{
    private readonly IProductSpecificationRepository _repository;

    public GetProductSpecificationByIdQueryHandler(IProductSpecificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProductSpecificationDto>> Handle(GetProductSpecificationByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = await _repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (spec is null)
            return Result.Failure<ProductSpecificationDto>(Error.NotFound(nameof(ProductSpecification), request.Id));

        return Result.Success(spec.Adapt<ProductSpecificationDto>());
    }
}
```

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Queries/
git commit -m "feat(catalog): add ProductSpecification queries"
```

---

### Task 1.9: API Endpoints

**Files:**
- Create: `src/Modules/ProductCatalog/Obss.ProductCatalog.Api/Endpoints/ProductSpecificationEndpoints.cs`

- [ ] **Create `ProductSpecificationEndpoints`**

File: `src/Modules/ProductCatalog/Obss.ProductCatalog.Api/Endpoints/ProductSpecificationEndpoints.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ProductCatalog.Application.Commands.AddCharacteristic;
using Obss.ProductCatalog.Application.Commands.AddCharacteristicValue;
using Obss.ProductCatalog.Application.Commands.AddSpecificationRelationship;
using Obss.ProductCatalog.Application.Commands.CreateProductSpecification;
using Obss.ProductCatalog.Application.Commands.DeleteProductSpecification;
using Obss.ProductCatalog.Application.Commands.PatchProductSpecification;
using Obss.ProductCatalog.Application.Commands.RemoveCharacteristic;
using Obss.ProductCatalog.Application.Commands.RemoveCharacteristicValue;
using Obss.ProductCatalog.Application.Commands.RemoveSpecificationRelationship;
using Obss.ProductCatalog.Application.Commands.UpdateCharacteristic;
using Obss.ProductCatalog.Application.Commands.UpdateCharacteristicValue;
using Obss.ProductCatalog.Application.Commands.UpdateProductSpecification;
using Obss.ProductCatalog.Application.Queries.GetProductSpecificationById;
using Obss.ProductCatalog.Application.Queries.GetProductSpecifications;

namespace Obss.ProductCatalog.Api.Endpoints;

public static class ProductSpecificationEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        // ProductSpecification CRUD
        group.MapPost("/product-specifications", async (CreateProductSpecificationCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/product-specifications/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/product-specifications/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductSpecificationByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/product-specifications", async ([AsParameters] GetProductSpecificationsQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            httpContext.Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
            httpContext.Response.Headers.Append("X-Result-Count", result.Value.Items.Count.ToString());
            return (IResult)TypedResults.Ok(result.Value.Items);
        });

        group.MapPut("/product-specifications/{id:guid}", async (Guid id, UpdateProductSpecificationCommand command, IMediator mediator) =>
        {
            if (id != command.ProductSpecificationId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPatch("/product-specifications/{id:guid}", async (Guid id, PatchProductSpecificationCommand command, IMediator mediator) =>
        {
            if (id != command.ProductSpecificationId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/product-specifications/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteProductSpecificationCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        });

        // Characteristics
        group.MapGet("/product-specifications/{specId:guid}/characteristics", async (Guid specId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductSpecificationByIdQuery(specId));
            if (!result.IsSuccess)
                return (IResult)TypedResults.NotFound(result.Error);
            return (IResult)TypedResults.Ok(result.Value.Characteristics);
        });

        group.MapPost("/product-specifications/{specId:guid}/characteristics", async (Guid specId, AddCharacteristicCommand command, IMediator mediator) =>
        {
            if (specId != command.ProductSpecificationId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/product-specifications/{specId}/characteristics/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/product-specifications/{specId:guid}/characteristics/{charId:guid}", async (Guid specId, Guid charId, UpdateCharacteristicCommand command, IMediator mediator) =>
        {
            if (specId != command.ProductSpecificationId || charId != command.CharacteristicId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/product-specifications/{specId:guid}/characteristics/{charId:guid}", async (Guid specId, Guid charId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveCharacteristicCommand(specId, charId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        // Characteristic Values
        group.MapGet("/product-specifications/{specId:guid}/characteristics/{charId:guid}/values", async (Guid specId, Guid charId, IMediator mediator) =>
        {
            var specResult = await mediator.Send(new GetProductSpecificationByIdQuery(specId));
            if (!specResult.IsSuccess)
                return (IResult)TypedResults.NotFound(specResult.Error);

            var characteristic = specResult.Value.Characteristics.FirstOrDefault(c => c.Id == charId);
            if (characteristic is null)
                return (IResult)TypedResults.NotFound();

            return (IResult)TypedResults.Ok(characteristic.Values);
        });

        group.MapPost("/product-specifications/{specId:guid}/characteristics/{charId:guid}/values", async (Guid specId, Guid charId, AddCharacteristicValueCommand command, IMediator mediator) =>
        {
            if (specId != command.ProductSpecificationId || charId != command.CharacteristicId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/product-specifications/{specId}/characteristics/{charId}/values/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/product-specifications/{specId:guid}/characteristics/{charId:guid}/values/{valueId:guid}", async (Guid specId, Guid charId, Guid valueId, UpdateCharacteristicValueCommand command, IMediator mediator) =>
        {
            if (specId != command.ProductSpecificationId || charId != command.CharacteristicId || valueId != command.ValueId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/product-specifications/{specId:guid}/characteristics/{charId:guid}/values/{valueId:guid}", async (Guid specId, Guid charId, Guid valueId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveCharacteristicValueCommand(specId, charId, valueId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        // Relationships
        group.MapGet("/product-specifications/{specId:guid}/relationships", async (Guid specId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductSpecificationByIdQuery(specId));
            if (!result.IsSuccess)
                return (IResult)TypedResults.NotFound(result.Error);
            return (IResult)TypedResults.Ok(result.Value.Relationships);
        });

        group.MapPost("/product-specifications/{specId:guid}/relationships", async (Guid specId, AddSpecificationRelationshipCommand command, IMediator mediator) =>
        {
            if (specId != command.ProductSpecificationId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/product-specifications/{specId}/relationships/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/product-specifications/{specId:guid}/relationships/{relId:guid}", async (Guid specId, Guid relId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveSpecificationRelationshipCommand(specId, relId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
```

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Api/Endpoints/ProductSpecificationEndpoints.cs
git commit -m "feat(catalog): add ProductSpecification API endpoints"
```

---

### Task 1.10: Update Module Registration

**Files:**
- Modify: `src/Modules/ProductCatalog/Obss.ProductCatalog.Api/Extensions/CatalogModuleRegistration.cs`

- [ ] **Register repository and map endpoints**

In `AddCatalogModule()`, add:
```csharp
services.AddScoped<IProductSpecificationRepository, ProductSpecificationRepository>();
```

In `MapCatalogEndpoints()`, add:
```csharp
ProductSpecificationEndpoints.Map(group);
```

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Api/Extensions/CatalogModuleRegistration.cs
git commit -m "feat(catalog): register ProductSpecification repository and endpoints"
```

---

### Task 1.11: Verify Build

- [ ] **Build the ProductCatalog API project**

Run:
```bash
dotnet build src/Modules/ProductCatalog/Obss.ProductCatalog.Api/Obss.ProductCatalog.Api.csproj --configuration Release
```

Expected: 0 errors. If there are compilation errors, fix them (likely missing imports in command/query files, or missing `PaginatedResult`/`using` statements).

- [ ] **Fix any issues and build again until clean**

---

### Task 1.12: DB Migration

**Files:**
- Generated: EF Core migration for `Tmf620ProductSpecification`

- [ ] **Create the migration**

Run:
```bash
~/.dotnet/tools/dotnet-ef migrations add Tmf620ProductSpecification -p src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Obss.ProductCatalog.Infrastructure.csproj -s src/Host/Obss.Host/Obss.Host.csproj -c CatalogDbContext --connection "Host=localhost;Database=obss_catalog;Username=obss;Password=obss123"
```

Verify migration files are generated under `Infrastructure/Persistence/Migrations/`.

- [ ] **Build to verify migration compiles**

```bash
dotnet build src/Modules/ProductCatalog/Obss.ProductCatalog.Api/Obss.ProductCatalog.Api.csproj --configuration Release
```

- [ ] **Apply the migration**

```bash
~/.dotnet/tools/dotnet-ef database update -p src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Obss.ProductCatalog.Infrastructure.csproj -s src/Host/Obss.Host/Obss.Host.csproj -c CatalogDbContext --connection "Host=localhost;Database=obss_catalog;Username=obss;Password=obss123"
```

- [ ] **Commit**

```bash
git add src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Persistence/Migrations/
git commit -m "feat(catalog): add Tmf620ProductSpecification migration"
```

---

### Task 1.13: Frontend Types

**Files:**
- Modify: `frontend/src/api/generated/dto.ts`
- Modify: `frontend/src/api/generated/index.ts`

- [ ] **Add frontend DTOs**

Add to `frontend/src/api/generated/dto.ts`:

```typescript
export interface ProductSpecificationDto {
  id: string;
  tenantId: string;
  name: string;
  description: string | null;
  brand: string | null;
  version: string | null;
  productNumber: string | null;
  lifecycleStatus: string;
  validFrom: string | null;
  validTo: string | null;
  createdAt: string;
  updatedAt: string;
  characteristics: ProductSpecificationCharacteristicDto[];
  relationships: ProductSpecificationRelationshipDto[];
}

export interface ProductSpecificationCharacteristicDto {
  id: string;
  productSpecificationId: string;
  name: string;
  description: string | null;
  valueType: string;
  configurable: boolean;
  minValue: number | null;
  maxValue: number | null;
  regex: string | null;
  sortOrder: number;
  maxCardinality: number | null;
  isRequired: boolean;
  values: ProductSpecificationCharacteristicValueDto[];
}

export interface ProductSpecificationCharacteristicValueDto {
  id: string;
  characteristicId: string;
  value: string;
  unitOfMeasure: string | null;
  isDefault: boolean;
  valueFrom: number | null;
  valueTo: number | null;
  rangeInterval: string | null;
  validFrom: string | null;
  validTo: string | null;
}

export interface ProductSpecificationRelationshipDto {
  id: string;
  productSpecificationId: string;
  targetSpecificationId: string;
  relationshipType: string;
  role: string | null;
  validFrom: string | null;
  validTo: string | null;
}
```

Update existing `ProductDto`:
```typescript
export interface ProductDto {
  // ...existing fields...
  productNumber: string | null;
  productSpecificationId: string | null;
  productSpecification: ProductSpecificationDto | null;
  // ...rest of existing fields...
}
```

- [ ] **Export new types from index.ts**

Add to the export type block in `frontend/src/api/generated/index.ts`:
```typescript
ProductSpecificationDto,
ProductSpecificationCharacteristicDto,
ProductSpecificationCharacteristicValueDto,
ProductSpecificationRelationshipDto,
```

- [ ] **Verify frontend builds**

```bash
cd frontend && bun run lint && cd ..
```

- [ ] **Commit**

```bash
git add frontend/src/api/generated/
git commit -m "feat(catalog): add ProductSpecification frontend types"
```

---

## Phase 2: ProductOfferingTerm

### Task 2.1: Domain + Infrastructure

**Files:**
- Create: `Obss.ProductCatalog.Domain/Domain/Enums/TermType.cs`
- Create: `Obss.ProductCatalog.Domain/Domain/Enums/DurationUnit.cs`
- Create: `Obss.ProductCatalog.Domain/Domain/Entities/ProductOfferingTerm.cs`
- Modify: `Obss.ProductCatalog.Domain/Domain/Entities/Offer.cs`
- Create: `Obss.ProductCatalog.Infrastructure/Persistence/Configurations/ProductOfferingTermConfiguration.cs`
- Modify: `Obss.ProductCatalog.Infrastructure/Persistence/Configurations/OfferConfiguration.cs`

- [ ] **Create `TermType` enum**

```csharp
namespace Obss.ProductCatalog.Domain.Domain.ValueObjects;

public enum TermType
{
    MinimumContract = 1,
    Renewal = 2,
    Cancellation = 3
}
```

- [ ] **Create `DurationUnit` enum**

```csharp
namespace Obss.ProductCatalog.Domain.Domain.ValueObjects;

public enum DurationUnit
{
    Days = 1,
    Months = 2,
    Years = 3
}
```

- [ ] **Create `ProductOfferingTerm` entity**

```csharp
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class ProductOfferingTerm : Entity<Guid>
{
    private ProductOfferingTerm() { }

    public ProductOfferingTerm(
        Guid id,
        Guid offerId,
        string name,
        string? description,
        int duration,
        DurationUnit durationUnit,
        TermType termType,
        DateTime? validFrom,
        DateTime? validTo)
        : base(id)
    {
        OfferId = offerId;
        Name = name;
        Description = description;
        Duration = duration;
        DurationUnit = durationUnit;
        TermType = termType;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public Guid OfferId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Duration { get; private set; }
    public DurationUnit DurationUnit { get; private set; }
    public TermType TermType { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    public void Update(string name, string? description, int duration, DurationUnit durationUnit, TermType termType, DateTime? validFrom, DateTime? validTo)
    {
        Name = name;
        Description = description;
        Duration = duration;
        DurationUnit = durationUnit;
        TermType = termType;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }
}
```

- [ ] **Add terms collection to `Offer`**

Add private backing field:
```csharp
private readonly List<ProductOfferingTerm> _terms = [];
```

Add read-only property:
```csharp
public IReadOnlyCollection<ProductOfferingTerm> Terms => _terms.AsReadOnly();
```

Add methods:
```csharp
public void AddTerm(ProductOfferingTerm term)
{
    _terms.Add(term);
    UpdatedAt = DateTime.UtcNow;
}

public void RemoveTerm(ProductOfferingTerm term)
{
    _terms.Remove(term);
    UpdatedAt = DateTime.UtcNow;
}
```

- [ ] **Create `ProductOfferingTermConfiguration`**

Table `product_offering_terms`, snake_case columns, `ValueGeneratedNever`, string conversion for enums, decimal(18,2) for Duration, cascade FK to Offer.

- [ ] **Add HasMany to OfferConfiguration**

Add after existing `HasMany(o => o.Discounts)`:
```csharp
builder.HasMany(o => o.Terms)
    .WithOne()
    .HasForeignKey(t => t.OfferId)
    .OnDelete(DeleteBehavior.Cascade);
```

- [ ] **Add migration** (named `Tmf620ProductOfferingTerm`)

- [ ] **Commit**

---

### Task 2.2: Application Layer

**Files:**
- Create: `Application/DTOs/ProductOfferingTermDto.cs`
- Create: `Application/Commands/AddProductOfferingTerm/*.cs` (command + handler + validator)
- Create: `Application/Commands/UpdateProductOfferingTerm/*.cs`
- Create: `Application/Commands/RemoveProductOfferingTerm/*.cs`
- Create: `Application/Queries/GetProductOfferingTerms/*.cs`
- Modify: `Application/DTOs/OfferDto.cs`
- Modify: `Application/Mappings/CatalogMappingConfig.cs`

- [ ] **Create `ProductOfferingTermDto`**

```csharp
using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Application.DTOs;

public sealed record ProductOfferingTermDto(
    Guid Id,
    Guid OfferId,
    string Name,
    string? Description,
    int Duration,
    DurationUnit DurationUnit,
    TermType TermType,
    DateTime? ValidFrom,
    DateTime? ValidTo);
```

- [ ] **Add Terms to OfferDto**

```csharp
List<ProductOfferingTermDto> Terms
```

- [ ] **Create commands + handlers**

`AddProductOfferingTermCommand`:
```csharp
public sealed record AddProductOfferingTermCommand(
    Guid OfferId,
    string Name,
    string? Description,
    int Duration,
    DurationUnit DurationUnit,
    TermType TermType,
    DateTime? ValidFrom,
    DateTime? ValidTo) : IRequest<Result<ProductOfferingTermDto>>;
```

Handler: fetch offer via `IOfferRepository` → create term → `offer.AddTerm()` → save → return mapped DTO.

- [ ] **Create queries**

`GetProductOfferingTermsQuery` → returns `List<ProductOfferingTermDto>`

Handler: fetch offer with terms → map → return.

- [ ] **Add Mapster mapping**

```csharp
TypeAdapterConfig<ProductOfferingTerm, ProductOfferingTermDto>.NewConfig()
    .Map(dest => dest.Id, src => src.Id);
```

Update Offer → OfferDto mapping to include Terms.

- [ ] **Commit**

---

### Task 2.3: API Endpoints

**Files:**
- Modify: `Api/Endpoints/OfferEndpoints.cs`

- [ ] **Add term endpoints to OfferEndpoints**

```csharp
group.MapGet("/offers/{offerId:guid}/terms", async (Guid offerId, IMediator mediator) =>
{
    var result = await mediator.Send(new GetProductOfferingTermsQuery(offerId));
    return result.IsSuccess
        ? (IResult)TypedResults.Ok(result.Value)
        : (IResult)TypedResults.NotFound(result.Error);
});

group.MapPost("/offers/{offerId:guid}/terms", async (Guid offerId, AddProductOfferingTermCommand command, IMediator mediator) =>
{
    if (offerId != command.OfferId)
        return (IResult)TypedResults.BadRequest();
    var result = await mediator.Send(command);
    return result.IsSuccess
        ? (IResult)TypedResults.Created($"/api/v1/catalog/offers/{offerId}/terms/{result.Value.Id}", result.Value)
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapPut("/offers/{offerId:guid}/terms/{termId:guid}", async (Guid offerId, Guid termId, UpdateProductOfferingTermCommand command, IMediator mediator) =>
{
    if (offerId != command.OfferId || termId != command.TermId)
        return (IResult)TypedResults.BadRequest();
    var result = await mediator.Send(command);
    return result.IsSuccess
        ? (IResult)TypedResults.Ok(result.Value)
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapDelete("/offers/{offerId:guid}/terms/{termId:guid}", async (Guid offerId, Guid termId, IMediator mediator) =>
{
    var result = await mediator.Send(new RemoveProductOfferingTermCommand(offerId, termId));
    return result.IsSuccess
        ? (IResult)TypedResults.NoContent()
        : (IResult)TypedResults.BadRequest(result.Error);
});
```

- [ ] **Build, verify 0 errors**

- [ ] **Add migration + apply** (named `Tmf620ProductOfferingTerm`)

- [ ] **Add frontend types**

```typescript
export interface ProductOfferingTermDto {
  id: string;
  offerId: string;
  name: string;
  description: string | null;
  duration: number;
  durationUnit: string;
  termType: string;
  validFrom: string | null;
  validTo: string | null;
}
```

Add `ProductOfferingTermDto` to index.ts exports.

- [ ] **Commit**

---

## Phase 3: BundledProductOffering

### Task 3.1: Domain + Infrastructure

- [ ] **Create `BundledProductOffering` entity**

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class BundledProductOffering : Entity<Guid>
{
    private BundledProductOffering() { }

    public BundledProductOffering(
        Guid id,
        Guid offerId,
        Guid bundledOfferId,
        string? name,
        int quantity,
        string? referralType)
        : base(id)
    {
        OfferId = offerId;
        BundledOfferId = bundledOfferId;
        Name = name;
        Quantity = quantity;
        ReferralType = referralType;
    }

    public Guid OfferId { get; private set; }
    public Guid BundledOfferId { get; private set; }
    public Offer? BundledOffer { get; }
    public string? Name { get; private set; }
    public int Quantity { get; private set; }
    public string? ReferralType { get; private set; }

    public void Update(string? name, int quantity, string? referralType)
    {
        Name = name;
        Quantity = quantity;
        ReferralType = referralType;
    }
}
```

- [ ] **Add bundled offerings collection to `Offer`**

```csharp
private readonly List<BundledProductOffering> _bundledOfferings = [];
public IReadOnlyCollection<BundledProductOffering> BundledOfferings => _bundledOfferings.AsReadOnly();

public void AddBundledOffering(BundledProductOffering bundledOffering)
{
    _bundledOfferings.Add(bundledOffering);
    UpdatedAt = DateTime.UtcNow;
}

public void RemoveBundledOffering(BundledProductOffering bundledOffering)
{
    _bundledOfferings.Remove(bundledOffering);
    UpdatedAt = DateTime.UtcNow;
}
```

- [ ] **Create `BundledProductOfferingConfiguration`**

Table `bundled_product_offerings`, unique (offer_id, bundled_offer_id), FK to Offer (OfferId cascade, BundledOfferId no cascade).

- [ ] **Add HasMany to OfferConfiguration**

- [ ] **Add migration** (named `Tmf620BundledProductOffering`)

---

### Task 3.2: Application Layer + API

- [ ] **Create `BundledProductOfferingDto`**

- [ ] **Add BundledOfferings to OfferDto**

- [ ] **Create commands:** `AddBundledProductOfferingCommand`, `UpdateBundledProductOfferingCommand`, `RemoveBundledProductOfferingCommand`

- [ ] **Create queries:** `GetBundledProductOfferingsQuery`

- [ ] **Add Mapster mapping**

- [ ] **Add endpoints** to OfferEndpoints: `/offers/{offerId}/bundled-offerings`

- [ ] **Build, verify 0 errors**

- [ ] **Add migration + apply**

- [ ] **Add frontend types** + export from index.ts

- [ ] **Commit**

---

## Phase 4: Pricing Enhancements

### Task 4.1: Domain + Infrastructure

**Files:**
- Modify: `.../Domain/Entities/OfferPricing.cs`
- Create: `.../Domain/Entities/PriceRange.cs`
- Create: `.../Infrastructure/Persistence/Configurations/PriceRangeConfiguration.cs`
- Modify: `.../Infrastructure/Persistence/Configurations/OfferPricingConfiguration.cs`
- Modify: `.../Infrastructure/Persistence/CatalogDbContext.cs`

- [ ] **Add Name, Description, PriceType, RecurringChargePeriod to OfferPricing**

Add properties:
```csharp
public string? Name { get; private set; }
public string? Description { get; private set; }
public PriceType? PriceType { get; private set; } // OneTime/Recurring/Usage — null is backward-compat
```

Note: It's valid to use the existing `PricingType` enum name here since the existing field is `PricingType` already. The new `PriceType` is a **different concept** (when the charge applies vs how it's calculated). Keep the naming distinct.

Actually, to avoid confusion with the existing `PricingType` property, use `ChargeApplicationType` or just keep the TMF name `PriceApplicationType`.

Let me use `PriceApplicationType`:
```csharp
public PriceApplicationType? PriceApplicationType { get; private set; }
```

With enum:
```csharp
public enum PriceApplicationType
{
    OneTime = 1,
    Recurring = 2,
    Usage = 3
}
```

And the existing `RecurringChargePeriod` is already represented by `BillingPeriod`. The new field is already present. So we only need `PriceApplicationType`.

Wait, looking at the design doc more carefully:
- `PriceType` (OneTime/Recurring/Usage) — when the charge applies
- `RecurringChargePeriod` (BillingPeriod) — when PriceType=Recurring

`RecurringChargePeriod` is the same type as `BillingPeriod` which already exists. So just add:
- `PriceApplicationType` (new enum)
- `Name` (string?)
- `Description` (string?)

These are pure additions, not replacing anything.

- [ ] **Create `PriceApplicationType` enum**

```csharp
namespace Obss.ProductCatalog.Domain.Domain.ValueObjects;

public enum PriceApplicationType
{
    OneTime = 1,
    Recurring = 2,
    Usage = 3
}
```

- [ ] **Create `PriceRange` entity**

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class PriceRange : Entity<Guid>
{
    private PriceRange() { }

    public PriceRange(Guid id, Guid offerPricingId, int minQuantity, int? maxQuantity, decimal price, bool isActive)
        : base(id)
    {
        OfferPricingId = offerPricingId;
        MinQuantity = minQuantity;
        MaxQuantity = maxQuantity;
        Price = price;
        IsActive = isActive;
    }

    public Guid OfferPricingId { get; private set; }
    public int MinQuantity { get; private set; }
    public int? MaxQuantity { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(int minQuantity, int? maxQuantity, decimal price, bool isActive)
    {
        MinQuantity = minQuantity;
        MaxQuantity = maxQuantity;
        Price = price;
        IsActive = isActive;
    }
}
```

- [ ] **Add PriceRanges collection to OfferPricing**

```csharp
private readonly List<PriceRange> _priceRanges = [];
public IReadOnlyCollection<PriceRange> PriceRanges => _priceRanges.AsReadOnly();

public void AddPriceRange(PriceRange range) { _priceRanges.Add(range); }
public void RemovePriceRange(PriceRange range) { _priceRanges.Remove(range); }
```

- [ ] **Create `PriceRangeConfiguration`**

Table `price_ranges`, FK cascade to OfferPricingId.

- [ ] **Add new columns to OfferPricingConfiguration**

```csharp
builder.Property(op => op.Name).HasColumnName("name").HasMaxLength(200);
builder.Property(op => op.Description).HasColumnName("description").HasMaxLength(2000);
builder.Property(op => op.PriceApplicationType)
    .HasColumnName("price_application_type")
    .HasConversion<string?>()
    .HasMaxLength(50);
```

Add HasMany for PriceRanges:
```csharp
builder.HasMany(op => op.PriceRanges)
    .WithOne()
    .HasForeignKey(pr => pr.OfferPricingId)
    .OnDelete(DeleteBehavior.Cascade);
```

- [ ] **Add migration** (named `Tmf620PricingEnhancements`)

---

### Task 4.2: Application Layer + API + Frontend

- [ ] **Create `PriceRangeDto`**

```csharp
public sealed record PriceRangeDto(
    Guid Id,
    Guid OfferPricingId,
    int MinQuantity,
    int? MaxQuantity,
    decimal Price,
    bool IsActive);
```

- [ ] **Add Name, Description, PriceApplicationType, PriceRanges to OfferPricingDto**

- [ ] **Add Mapster mappings**

- [ ] **Create commands:** `AddPriceRangeCommand`, `UpdatePriceRangeCommand`, `RemovePriceRangeCommand`

- [ ] **Create queries:** `GetPriceRangesQuery`

- [ ] **Add endpoints** to OfferEndpoints: `/offers/{offerId}/pricing/{pricingId}/price-ranges`

- [ ] **Build, verify 0 errors**

- [ ] **Add migration + apply**

- [ ] **Add frontend types** + export from index.ts

- [ ] **Commit**

---

## Verification

### Final Build Check

```bash
dotnet build src/Modules/ProductCatalog/Obss.ProductCatalog.Api/Obss.ProductCatalog.Api.csproj --configuration Release
```

### Final Frontend Check

```bash
cd frontend && bun run lint && cd ..
```

### Git Status

```bash
git status
git log --oneline -10
```
