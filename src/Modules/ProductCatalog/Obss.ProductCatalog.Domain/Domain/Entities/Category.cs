using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class Category : AggregateRoot<Guid>, ITenantEntity
{
    private Category() { }

    public Category(
        Guid id,
        string tenantId,
        string name,
        string? description,
        Guid? parentCategoryId,
        int sortOrder)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;
        IsActive = true;
        LifecycleStatus = LifecycleStatus.Active;
        SortOrder = sortOrder;
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public LifecycleStatus LifecycleStatus { get; private set; }
    public int SortOrder { get; private set; }
    public int Version { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public bool IsRoot => !ParentCategoryId.HasValue;

    public static Category Create(
        string tenantId,
        string name,
        string? description,
        Guid? parentCategoryId,
        int sortOrder)
    {
        return new Category(Guid.NewGuid(), tenantId, name, description, parentCategoryId, sortOrder);
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        LifecycleStatus = LifecycleStatus.Active;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        LifecycleStatus = LifecycleStatus.Retired;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveTo(Category parent)
    {
        ParentCategoryId = parent.Id;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveToParent(Guid? parentCategoryId)
    {
        ParentCategoryId = parentCategoryId;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description, int sortOrder)
    {
        Name = name;
        Description = description;
        SortOrder = sortOrder;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetValidityPeriod(DateTime? validFrom, DateTime? validTo)
    {
        ValidFrom = validFrom;
        ValidTo = validTo;
        UpdatedAt = DateTime.UtcNow;
    }
}
