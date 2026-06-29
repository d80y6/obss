using Obss.SharedKernel.Domain.Common;

namespace Obss.Workflow.Domain.Entities;

public class SlaDefinition : AggregateRoot<Guid>
{
    private SlaDefinition() { }

    private SlaDefinition(
        Guid id,
        string tenantId,
        string name,
        string? description,
        string targetType,
        TimeSpan targetDuration,
        TimeSpan escalationDuration,
        decimal penaltyAmount,
        string penaltyCurrency)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        TargetType = targetType;
        TargetDuration = targetDuration;
        EscalationDuration = escalationDuration;
        PenaltyAmount = penaltyAmount;
        PenaltyCurrency = penaltyCurrency;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string TargetType { get; private set; } = string.Empty;
    public TimeSpan TargetDuration { get; private set; }
    public TimeSpan EscalationDuration { get; private set; }
    public decimal PenaltyAmount { get; private set; }
    public string PenaltyCurrency { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static SlaDefinition Create(
        string tenantId,
        string name,
        string? description,
        string targetType,
        TimeSpan targetDuration,
        TimeSpan escalationDuration,
        decimal penaltyAmount,
        string penaltyCurrency)
    {
        return new SlaDefinition(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            targetType,
            targetDuration,
            escalationDuration,
            penaltyAmount,
            penaltyCurrency);
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
