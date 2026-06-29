using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Domain.Entities;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Infrastructure.Persistence.Repositories;

public sealed class WorkflowMetricRepository : EfRepository<WorkflowMetric>, IWorkflowMetricRepository
{
    public WorkflowMetricRepository(WorkflowDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<WorkflowMetric>> GetMetricsAsync(
        Guid? workflowDefinitionId,
        MetricType? metricType,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (workflowDefinitionId.HasValue)
            query = query.Where(m => m.WorkflowDefinitionId == workflowDefinitionId.Value);

        if (metricType.HasValue)
            query = query.Where(m => m.MetricType == metricType.Value);

        if (from.HasValue)
            query = query.Where(m => m.TimeBucket >= from.Value);

        if (to.HasValue)
            query = query.Where(m => m.TimeBucket <= to.Value);

        query = query.OrderByDescending(m => m.TimeBucket);

        return await query.ToListAsync(cancellationToken);
    }
}
