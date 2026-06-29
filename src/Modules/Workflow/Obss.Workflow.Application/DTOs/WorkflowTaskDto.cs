namespace Obss.Workflow.Application.DTOs;

public sealed record WorkflowTaskDto(
    Guid Id,
    Guid WorkflowInstanceId,
    Guid WorkflowStepId,
    string StepName,
    string Status,
    string? AssignedTo,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? Result,
    string? ErrorMessage,
    int RetryCount);
