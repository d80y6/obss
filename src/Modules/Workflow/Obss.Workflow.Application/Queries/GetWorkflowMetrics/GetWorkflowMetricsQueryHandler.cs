using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Application.Queries.GetWorkflowMetrics;

public sealed class GetWorkflowMetricsQueryHandler : IRequestHandler<GetWorkflowMetricsQuery, Result<IReadOnlyList<WorkflowMetricDto>>>
{
    private readonly IWorkflowMetricRepository _repository;

    public GetWorkflowMetricsQueryHandler(IWorkflowMetricRepository repository)
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

        var metrics = await _repository.GetMetricsAsync(
            request.WorkflowDefinitionId,
            metricType,
            request.From,
            request.To,
            cancellationToken);

        var result = metrics.Adapt<List<WorkflowMetricDto>>();
        return Result.Success<IReadOnlyList<WorkflowMetricDto>>(result);
    }
}
