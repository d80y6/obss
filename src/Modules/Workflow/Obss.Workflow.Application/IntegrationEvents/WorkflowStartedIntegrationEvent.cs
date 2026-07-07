using Obss.SharedKernel.Domain.Events;

namespace Obss.Workflow.Application.IntegrationEvents;

public sealed class WorkflowStartedIntegrationEvent : IntegrationEvent
{
    public WorkflowStartedIntegrationEvent(
        Guid provisioningJobId,
        Guid workflowInstanceId)
    {
        ProvisioningJobId = provisioningJobId;
        WorkflowInstanceId = workflowInstanceId;
    }

    public Guid ProvisioningJobId { get; }
    public Guid WorkflowInstanceId { get; }
}
