using Obss.SharedKernel.Application.Abstractions;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Abstractions;

public interface IWorkflowInstanceRepository : IRepository<WorkflowInstance>
{
    Task<IReadOnlyList<WorkflowInstance>> GetFilteredAsync(
        string? status,
        string? entityType,
        Guid? entityId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkflowTaskInstance>> GetPendingTasksAsync(CancellationToken cancellationToken = default);
}
