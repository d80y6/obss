using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Application.DTOs;

public sealed record WorkflowDashboardDto(
    int TotalRunning,
    int TotalCompleted,
    int TotalFailed,
    int TotalPending,
    int TotalPaused,
    int TotalCancelled,
    double AverageDurationSeconds,
    double FailureRate,
    double SlaComplianceRate,
    List<RecentWorkflowDto> RecentlyCompleted);

public sealed record RecentWorkflowDto(
    Guid Id,
    string WorkflowDefinitionName,
    string TriggerEntityType,
    DateTime? CompletedAt,
    InstanceStatus Status);
