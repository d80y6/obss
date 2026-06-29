using Obss.SharedKernel.Domain.Common;

namespace Obss.Workflow.Domain.Events;

public sealed class WorkflowCompletedDomainEvent : DomainEvent
{
    public WorkflowCompletedDomainEvent(Guid workflowInstanceId, Guid workflowDefinitionId)
    {
        WorkflowInstanceId = workflowInstanceId;
        WorkflowDefinitionId = workflowDefinitionId;
    }

    public Guid WorkflowInstanceId { get; }
    public Guid WorkflowDefinitionId { get; }
}
