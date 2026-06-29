using Obss.SharedKernel.Application.Abstractions;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Abstractions;

public interface IWorkflowSlaRepository : IRepository<WorkflowSla>
{
    Task<IReadOnlyList<WorkflowSla>> GetByWorkflowDefinitionIdAsync(Guid workflowDefinitionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowSla>> GetActiveSlasAsync(CancellationToken cancellationToken = default);
}
