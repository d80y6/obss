using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetWorkflowDefinitionById;

public sealed record GetWorkflowDefinitionByIdQuery(Guid Id) : IRequest<Result<WorkflowDefinitionDto>>;
