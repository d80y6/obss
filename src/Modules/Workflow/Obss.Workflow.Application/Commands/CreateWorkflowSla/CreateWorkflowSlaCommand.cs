using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Commands.CreateWorkflowSla;

public sealed record CreateWorkflowSlaCommand(
    string TenantId,
    string Name,
    string? Description,
    Guid WorkflowDefinitionId,
    int TargetDurationMinutes,
    decimal WarningThresholdPercent,
    string? EscalationUserId,
    string? EscalationGroup) : IRequest<Result<WorkflowSlaDto>>;
