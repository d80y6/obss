using Obss.SharedKernel.Domain.Common;

namespace Obss.Workflow.Domain.Events;

public sealed class WorkflowFailedDomainEvent : DomainEvent
{
    public WorkflowFailedDomainEvent(Guid workflowInstanceId, Guid workflowDefinitionId, string error)
    {
        WorkflowInstanceId = workflowInstanceId;
        WorkflowDefinitionId = workflowDefinitionId;
        Error = error;
    }

    public Guid WorkflowInstanceId { get; }
    public Guid WorkflowDefinitionId { get; }
    public string Error { get; }
}
