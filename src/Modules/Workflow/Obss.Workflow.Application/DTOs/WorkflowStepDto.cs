namespace Obss.Workflow.Application.DTOs;

public sealed record WorkflowStepDto(
    Guid Id,
    Guid WorkflowDefinitionId,
    int StepNumber,
    string Name,
    string? Description,
    string StepType,
    string? HandlerType,
    string? Configuration,
    int Timeout,
    int RetryCount,
    int RetryDelaySeconds,
    bool IsRequired,
    DateTime CreatedAt);
