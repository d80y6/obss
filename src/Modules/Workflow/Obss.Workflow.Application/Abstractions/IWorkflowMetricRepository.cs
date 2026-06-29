using Obss.SharedKernel.Application.Abstractions;
using Obss.Workflow.Domain.Entities;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Application.Abstractions;

public interface IWorkflowMetricRepository : IRepository<WorkflowMetric>
{
    Task<IReadOnlyList<WorkflowMetric>> GetMetricsAsync(Guid? workflowDefinitionId, MetricType? metricType, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
