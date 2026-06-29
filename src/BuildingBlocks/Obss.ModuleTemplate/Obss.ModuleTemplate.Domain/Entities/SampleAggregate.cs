using Obss.SharedKernel.Domain.Common;
using Obss.ModuleTemplate.Domain.Events;

namespace Obss.ModuleTemplate.Domain.Entities;

public class SampleAggregate : AggregateRoot<Guid>
{
    private SampleAggregate() { }

    private SampleAggregate(Guid id, string name, string tenantId)
        : base(id)
    {
        Name = name;
        TenantId = tenantId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new SampleCreatedDomainEvent(id, name));
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static SampleAggregate Create(string name, string tenantId, string? description = null)
    {
        return new SampleAggregate(Guid.NewGuid(), name, tenantId)
        {
            Description = description
        };
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
