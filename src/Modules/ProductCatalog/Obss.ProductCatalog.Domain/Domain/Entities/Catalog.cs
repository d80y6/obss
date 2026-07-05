using Obss.ProductCatalog.Domain.Domain.Events;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class Catalog : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<Category> _categories = [];

    private Catalog() { }

    private Catalog(
        Guid id,
        string tenantId,
        string name,
        string? description,
        string? catalogType,
        int version,
        DateTime? validFrom,
        DateTime? validTo)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        CatalogType = catalogType;
        LifecycleStatus = LifecycleStatus.Draft;
        Version = version;
        ValidFrom = validFrom;
        ValidTo = validTo;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CatalogCreatedDomainEvent(id, tenantId, name));
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? CatalogType { get; private set; }
    public LifecycleStatus LifecycleStatus { get; private set; }
    public int Version { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    public static Catalog Create(
        string tenantId,
        string name,
        string? description,
        string? catalogType,
        int version,
        DateTime? validFrom,
        DateTime? validTo)
    {
        return new Catalog(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            catalogType,
            version,
            validFrom,
            validTo);
    }

    public void UpdateDetails(
        string name,
        string? description,
        string? catalogType,
        int version,
        DateTime? validFrom,
        DateTime? validTo)
    {
        Name = name;
        Description = description;
        CatalogType = catalogType;
        Version = version;
        ValidFrom = validFrom;
        ValidTo = validTo;
        UpdatedAt = DateTime.UtcNow;
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
}
