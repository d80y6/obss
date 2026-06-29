using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Commands.ApplyWorkflowSla;

public sealed record ApplyWorkflowSlaCommand(Guid WorkflowInstanceId, Guid WorkflowSlaId) : IRequest<Result<WorkflowInstanceDto>>;
