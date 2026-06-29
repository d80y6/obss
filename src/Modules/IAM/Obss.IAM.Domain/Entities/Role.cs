using Obss.SharedKernel.Domain.Common;

namespace Obss.IAM.Domain.Entities;

public class Role : Entity<Guid>
{
    private Role() { }

    public Role(
        Guid id,
        string tenantId,
        string name,
        string? description,
        bool isSystem = false)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        IsSystem = isSystem;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<Permission> _permissions = [];
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    public void AddPermission(Permission permission)
    {
        if (!_permissions.Contains(permission))
            _permissions.Add(permission);
    }

    public void RemovePermission(Permission permission)
    {
        _permissions.Remove(permission);
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
