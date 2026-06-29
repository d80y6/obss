namespace Obss.Workflow.Application.DTOs;

public sealed record WorkflowInstanceDto(
    Guid Id,
    Guid WorkflowDefinitionId,
    string WorkflowDefinitionName,
    string TriggerEntityType,
    Guid TriggerEntityId,
    string Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string CreatedBy,
    List<WorkflowTaskDto> Tasks);
