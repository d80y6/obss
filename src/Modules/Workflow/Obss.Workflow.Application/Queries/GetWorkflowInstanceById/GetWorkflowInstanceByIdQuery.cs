using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetWorkflowInstanceById;

public sealed record GetWorkflowInstanceByIdQuery(Guid Id) : IRequest<Result<WorkflowInstanceDto>>;
