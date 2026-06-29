using Obss.SharedKernel.Domain.Common;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Domain.Entities;

public class WorkflowMetric : Entity<Guid>
{
    private WorkflowMetric() { }

    public WorkflowMetric(
        Guid id,
        MetricType metricType,
        Guid workflowDefinitionId,
        DateTime timeBucket,
        int count,
        decimal value)
        : base(id)
    {
        MetricType = metricType;
        WorkflowDefinitionId = workflowDefinitionId;
        TimeBucket = timeBucket;
        Count = count;
        Value = value;
        RecordedAt = DateTime.UtcNow;
    }

    public MetricType MetricType { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public DateTime TimeBucket { get; private set; }
    public int Count { get; private set; }
    public decimal Value { get; private set; }
    public DateTime RecordedAt { get; private set; }
}
