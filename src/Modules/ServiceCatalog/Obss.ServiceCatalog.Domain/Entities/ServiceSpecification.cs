using Obss.ServiceCatalog.Domain.Enums;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

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
