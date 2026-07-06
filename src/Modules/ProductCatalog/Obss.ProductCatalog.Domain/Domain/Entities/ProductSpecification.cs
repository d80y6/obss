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
        string productNumber,
        LifecycleStatus lifecycleStatus,
        DateTime? validFrom,
        DateTime? validTo,
        Guid? serviceSpecificationId = null)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        Brand = brand;
        Version = version;
        ProductNumber = productNumber;
        LifecycleStatus = lifecycleStatus;
        ValidFrom = validFrom;
        ValidTo = validTo;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        ServiceSpecificationId = serviceSpecificationId;

        AddDomainEvent(new ProductSpecificationCreatedDomainEvent(id, tenantId, name));
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Brand { get; private set; }
    public string? Version { get; private set; }
    public string ProductNumber { get; private set; } = string.Empty;
    public LifecycleStatus LifecycleStatus { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? ServiceSpecificationId { get; private set; }

    public IReadOnlyCollection<ProductSpecificationCharacteristic> Characteristics => _characteristics.AsReadOnly();
    public IReadOnlyCollection<ProductSpecificationRelationship> Relationships => _relationships.AsReadOnly();

    public static ProductSpecification Create(
        string tenantId,
        string name,
        string? description,
        string? brand,
        string? version,
        string productNumber,
        Guid? serviceSpecificationId = null)
    {
        return new ProductSpecification(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            brand,
            version,
            productNumber,
            LifecycleStatus.Draft,
            null,
            null,
            serviceSpecificationId);
    }

    public void Activate()
    {
        if (LifecycleStatus == LifecycleStatus.Active)
            return;

        LifecycleStatus = LifecycleStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Retire()
    {
        if (LifecycleStatus == LifecycleStatus.Retired)
            return;

        LifecycleStatus = LifecycleStatus.Retired;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Discontinue()
    {
        if (LifecycleStatus == LifecycleStatus.Discontinued)
            return;

        LifecycleStatus = LifecycleStatus.Discontinued;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string name,
        string? description,
        string? brand,
        string? version,
        string productNumber)
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
