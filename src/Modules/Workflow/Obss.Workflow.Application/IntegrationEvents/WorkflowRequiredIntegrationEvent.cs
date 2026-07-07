using Obss.SharedKernel.Domain.Events;

namespace Obss.Workflow.Application.IntegrationEvents;

public sealed class WorkflowRequiredIntegrationEvent : IntegrationEvent
{
    public WorkflowRequiredIntegrationEvent(
        Guid provisioningJobId,
        Guid workflowDefinitionId,
        string triggerEntityType,
        string createdBy)
    {
        ProvisioningJobId = provisioningJobId;
        WorkflowDefinitionId = workflowDefinitionId;
        TriggerEntityType = triggerEntityType;
        CreatedBy = createdBy;
    }

    public Guid ProvisioningJobId { get; }
    public Guid WorkflowDefinitionId { get; }
    public string TriggerEntityType { get; }
    public string CreatedBy { get; }
}
