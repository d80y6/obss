using Obss.SharedKernel.Domain.Common;

namespace Obss.Workflow.Domain.Events;

public sealed class WorkflowSlaBreachedDomainEvent : DomainEvent
{
    public WorkflowSlaBreachedDomainEvent(Guid workflowInstanceId, Guid workflowDefinitionId, DateTime slaDeadline)
    {
        WorkflowInstanceId = workflowInstanceId;
        WorkflowDefinitionId = workflowDefinitionId;
        SlaDeadline = slaDeadline;
    }

    public Guid WorkflowInstanceId { get; }
    public Guid WorkflowDefinitionId { get; }
    public DateTime SlaDeadline { get; }
}
