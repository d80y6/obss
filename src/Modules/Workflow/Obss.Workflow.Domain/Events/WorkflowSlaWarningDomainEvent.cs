using Obss.SharedKernel.Domain.Common;

namespace Obss.Workflow.Domain.Events;

public sealed class WorkflowSlaWarningDomainEvent : DomainEvent
{
    public WorkflowSlaWarningDomainEvent(Guid workflowInstanceId, Guid workflowDefinitionId, DateTime slaDeadline, decimal warningThresholdPercent)
    {
        WorkflowInstanceId = workflowInstanceId;
        WorkflowDefinitionId = workflowDefinitionId;
        SlaDeadline = slaDeadline;
        WarningThresholdPercent = warningThresholdPercent;
    }

    public Guid WorkflowInstanceId { get; }
    public Guid WorkflowDefinitionId { get; }
    public DateTime SlaDeadline { get; }
    public decimal WarningThresholdPercent { get; }
}
