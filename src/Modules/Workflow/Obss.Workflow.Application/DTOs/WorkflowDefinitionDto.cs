namespace Obss.Workflow.Application.DTOs;

public sealed record WorkflowDefinitionDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string Category,
    bool IsActive,
    int Version,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<WorkflowStepDto> Steps);
