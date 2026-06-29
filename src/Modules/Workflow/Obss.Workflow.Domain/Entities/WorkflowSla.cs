using Obss.SharedKernel.Domain.Common;

namespace Obss.Workflow.Domain.Entities;

public class WorkflowSla : AggregateRoot<Guid>
{
    private WorkflowSla() { }

    private WorkflowSla(
        Guid id,
        string tenantId,
        string name,
        string? description,
        Guid workflowDefinitionId,
        int targetDurationMinutes,
        decimal warningThresholdPercent,
        string? escalationUserId,
        string? escalationGroup)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        WorkflowDefinitionId = workflowDefinitionId;
        TargetDurationMinutes = targetDurationMinutes;
        WarningThresholdPercent = warningThresholdPercent;
        EscalationUserId = escalationUserId;
        EscalationGroup = escalationGroup;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public int TargetDurationMinutes { get; private set; }
    public decimal WarningThresholdPercent { get; private set; }
    public string? EscalationUserId { get; private set; }
    public string? EscalationGroup { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static WorkflowSla Create(
        string tenantId,
        string name,
        string? description,
        Guid workflowDefinitionId,
        int targetDurationMinutes,
        decimal warningThresholdPercent,
        string? escalationUserId,
        string? escalationGroup)
    {
        return new WorkflowSla(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            workflowDefinitionId,
            targetDurationMinutes,
            warningThresholdPercent,
            escalationUserId,
            escalationGroup);
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
