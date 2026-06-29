using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Commands.AddWorkflowStep;

public sealed record AddWorkflowStepCommand(
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
    bool IsRequired) : IRequest<Result<WorkflowDefinitionDto>>;
