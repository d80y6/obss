using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetWorkflowMetrics;

public sealed record GetWorkflowMetricsQuery(
    Guid? WorkflowDefinitionId,
    string? MetricType,
    DateTime? From,
    DateTime? To) : IRequest<Result<IReadOnlyList<WorkflowMetricDto>>>;
