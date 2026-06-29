using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Infrastructure.Persistence.Repositories;

public sealed class WorkflowSlaRepository : EfRepository<WorkflowSla>, IWorkflowSlaRepository
{
    public WorkflowSlaRepository(WorkflowDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<WorkflowSla>> GetByWorkflowDefinitionIdAsync(Guid workflowDefinitionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.WorkflowDefinitionId == workflowDefinitionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowSla>> GetActiveSlasAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);
    }
}
