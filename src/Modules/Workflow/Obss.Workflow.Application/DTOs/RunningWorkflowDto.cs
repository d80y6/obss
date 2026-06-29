namespace Obss.Workflow.Application.DTOs;

public sealed record RunningWorkflowDto(
    Guid Id,
    Guid WorkflowDefinitionId,
    string WorkflowDefinitionName,
    string TriggerEntityType,
    Guid TriggerEntityId,
    DateTime StartedAt,
    DateTime? SlaDeadline,
    bool IsSlaBreached,
    DateTime? SlaBreachedAt,
    int TaskCount,
    int CompletedTaskCount);
