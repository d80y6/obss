using Obss.SharedKernel.Domain.Common;

namespace Obss.Workflow.Domain.Events;

public sealed class WorkflowStartedDomainEvent : DomainEvent
{
    public WorkflowStartedDomainEvent(Guid workflowInstanceId, Guid workflowDefinitionId, string triggerEntityType, Guid triggerEntityId)
    {
        WorkflowInstanceId = workflowInstanceId;
        WorkflowDefinitionId = workflowDefinitionId;
        TriggerEntityType = triggerEntityType;
        TriggerEntityId = triggerEntityId;
    }

    public Guid WorkflowInstanceId { get; }
    public Guid WorkflowDefinitionId { get; }
    public string TriggerEntityType { get; }
    public Guid TriggerEntityId { get; }
}
