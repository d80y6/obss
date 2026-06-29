using Obss.SharedKernel.Domain.Common;
using Obss.Ticketing.Domain.ValueObjects;

namespace Obss.Ticketing.Domain.Entities;

public class SlaDefinition : AggregateRoot<Guid>
{
    private SlaDefinition() { }

    private SlaDefinition(
        Guid id,
        string tenantId,
        string name,
        string? description,
        TicketPriority priority,
        int responseTimeHours,
        int resolutionTimeHours,
        int escalationTimeHours)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        Priority = priority;
        ResponseTimeHours = responseTimeHours;
        ResolutionTimeHours = resolutionTimeHours;
        EscalationTimeHours = escalationTimeHours;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TicketPriority Priority { get; private set; }
    public int ResponseTimeHours { get; private set; }
    public int ResolutionTimeHours { get; private set; }
    public int EscalationTimeHours { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static SlaDefinition Create(
        string tenantId,
        string name,
        string? description,
        TicketPriority priority,
        int responseTimeHours,
        int resolutionTimeHours,
        int escalationTimeHours)
    {
        return new SlaDefinition(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            priority,
            responseTimeHours,
            resolutionTimeHours,
            escalationTimeHours);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
