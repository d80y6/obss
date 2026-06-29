namespace Obss.Workflow.Application.DTOs;

public sealed record WorkflowMetricDto(
    Guid Id,
    string MetricType,
    Guid WorkflowDefinitionId,
    DateTime TimeBucket,
    int Count,
    decimal Value,
    DateTime RecordedAt);
