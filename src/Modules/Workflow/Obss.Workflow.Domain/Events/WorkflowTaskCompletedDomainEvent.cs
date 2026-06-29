using Obss.SharedKernel.Domain.Common;

namespace Obss.Workflow.Domain.Events;

public sealed class WorkflowTaskCompletedDomainEvent : DomainEvent
{
    public WorkflowTaskCompletedDomainEvent(Guid taskInstanceId, Guid workflowInstanceId, Guid workflowStepId)
    {
        TaskInstanceId = taskInstanceId;
        WorkflowInstanceId = workflowInstanceId;
        WorkflowStepId = workflowStepId;
    }

    public Guid TaskInstanceId { get; }
    public Guid WorkflowInstanceId { get; }
    public Guid WorkflowStepId { get; }
}
