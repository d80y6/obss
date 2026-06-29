using Obss.SharedKernel.Application.Abstractions;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Abstractions;

public interface IWorkflowDefinitionRepository : IRepository<WorkflowDefinition>
{
    Task<IReadOnlyList<WorkflowDefinition>> GetFilteredAsync(
        string? tenantId,
        string? category,
        bool? isActive,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
