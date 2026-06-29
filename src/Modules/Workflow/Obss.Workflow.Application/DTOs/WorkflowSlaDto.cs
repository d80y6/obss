namespace Obss.Workflow.Application.DTOs;

public sealed record WorkflowSlaDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    Guid WorkflowDefinitionId,
    int TargetDurationMinutes,
    decimal WarningThresholdPercent,
    string? EscalationUserId,
    string? EscalationGroup,
    bool IsActive,
    DateTime CreatedAt);
