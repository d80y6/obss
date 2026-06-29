using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Domain.Services;

public interface IWorkflowEngine
{
    Task<WorkflowInstance> StartWorkflow(Guid definitionId, string entityType, Guid entityId, string createdBy, CancellationToken cancellationToken = default);
    Task<WorkflowTaskInstance> ExecuteStep(Guid instanceId, Guid taskId, CancellationToken cancellationToken = default);
}
