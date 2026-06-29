using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Infrastructure.Persistence.Repositories;

public sealed class WorkflowInstanceRepository : EfRepository<WorkflowInstance>, IWorkflowInstanceRepository
{
    public WorkflowInstanceRepository(WorkflowDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<WorkflowInstance>> GetFilteredAsync(
        string? status,
        string? entityType,
        Guid? entityId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status.ToString() == status);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(i => i.TriggerEntityType == entityType);

        if (entityId.HasValue)
            query = query.Where(i => i.TriggerEntityId == entityId.Value);

        query = query
            .OrderByDescending(i => i.StartedAt)
            .ThenByDescending(i => i.CreatedBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowTaskInstance>> GetPendingTasksAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<WorkflowTaskInstance>()
            .Where(t => t.Status == Domain.ValueObjects.WorkflowTaskStatus.Pending)
            .OrderBy(t => t.WorkflowInstanceId)
            .ToListAsync(cancellationToken);
    }
}
