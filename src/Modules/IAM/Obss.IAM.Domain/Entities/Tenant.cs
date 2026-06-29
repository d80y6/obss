using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.IAM.Domain.Events;

namespace Obss.IAM.Domain.Entities;

public class Tenant : AggregateRoot<Guid>
{
    private Tenant() { }

    private Tenant(Guid id, string name, string slug, string? connectionString, string? settings)
        : base(id)
    {
        Name = name;
        Slug = slug;
        ConnectionString = connectionString;
        Settings = settings;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new TenantProvisionedDomainEvent(TenantId.Create(id.ToString("N")), name, slug));
    }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? ConnectionString { get; private set; }
    public bool IsActive { get; private set; }
    public string? Settings { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static Tenant Create(string name, string slug, string? connectionString = null, string? settings = null)
    {
        return new Tenant(Guid.NewGuid(), name, slug, connectionString, settings);
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSettings(string settings)
    {
        Settings = settings;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateConnectionString(string connectionString)
    {
        ConnectionString = connectionString;
        UpdatedAt = DateTime.UtcNow;
    }
}
