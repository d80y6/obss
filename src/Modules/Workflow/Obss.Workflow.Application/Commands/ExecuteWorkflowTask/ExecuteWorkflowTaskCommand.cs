using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Commands.ExecuteWorkflowTask;

public sealed record ExecuteWorkflowTaskCommand(
    Guid InstanceId,
    Guid TaskId) : IRequest<Result<WorkflowTaskDto>>;
