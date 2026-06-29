using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Commands.RemoveWorkflowStep;

public sealed record RemoveWorkflowStepCommand(
    Guid WorkflowDefinitionId,
    Guid StepId) : IRequest<Result<WorkflowDefinitionDto>>;
