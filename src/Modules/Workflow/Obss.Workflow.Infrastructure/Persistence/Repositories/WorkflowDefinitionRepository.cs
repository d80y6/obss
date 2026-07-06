using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Infrastructure.Persistence.Repositories;

public sealed class WorkflowDefinitionRepository : EfRepository<WorkflowDefinition>, IWorkflowDefinitionRepository
{
    public WorkflowDefinitionRepository(WorkflowDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<WorkflowDefinition>> GetFilteredAsync(
        string? tenantId,
        string? category,
        bool? isActive,
        string? searchTerm,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
            query = query.Where(w => w.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(w => w.Category.ToString() == category);

        if (isActive.HasValue)
            query = query.Where(w => w.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(w =>
                w.Name.Contains(searchTerm) ||
                (w.Description != null && w.Description.Contains(searchTerm)));

        query = query
            .OrderByDescending(w => w.Version)
            .ThenBy(w => w.Name)
            .Skip(offset)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }
}
