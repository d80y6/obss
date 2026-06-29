using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class Category : Entity<Guid>, ITenantEntity
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
        SortOrder = sortOrder;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }

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
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
    }

    public void MoveTo(Category parent)
    {
        ParentCategoryId = parent.Id;
    }

    public void UpdateDetails(string name, string? description, int sortOrder)
    {
        Name = name;
        Description = description;
        SortOrder = sortOrder;
    }
}
