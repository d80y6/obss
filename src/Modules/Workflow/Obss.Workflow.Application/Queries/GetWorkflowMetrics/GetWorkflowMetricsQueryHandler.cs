using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Application.Queries.GetWorkflowMetrics;

public sealed class GetWorkflowMetricsQueryHandler : IRequestHandler<GetWorkflowMetricsQuery, Result<IReadOnlyList<WorkflowMetricDto>>>
{
    private readonly IRepository<WorkflowMetric> _repository;

    public GetWorkflowMetricsQueryHandler(IRepository<WorkflowMetric> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<WorkflowMetricDto>>> Handle(GetWorkflowMetricsQuery request, CancellationToken cancellationToken)
    {
        MetricType? metricType = null;
        if (!string.IsNullOrWhiteSpace(request.MetricType) &&
            Enum.TryParse<MetricType>(request.MetricType, true, out var parsed))
        {
            metricType = parsed;
        }

        var query = _repository.GetQueryable();

        if (request.WorkflowDefinitionId.HasValue)
            query = query.Where(m => m.WorkflowDefinitionId == request.WorkflowDefinitionId.Value);

        if (metricType.HasValue)
            query = query.Where(m => m.MetricType == metricType.Value);

        if (request.From.HasValue)
            query = query.Where(m => m.TimeBucket >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(m => m.TimeBucket <= request.To.Value);

        query = query.OrderByDescending(m => m.TimeBucket);

        var metrics = await query.ToListAsync(cancellationToken);
        var result = metrics.Adapt<List<WorkflowMetricDto>>();
        return Result.Success<IReadOnlyList<WorkflowMetricDto>>(result);
    }
}
