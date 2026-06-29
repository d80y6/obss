using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetWorkflowSlas;

public sealed record GetWorkflowSlasQuery(Guid? WorkflowDefinitionId) : IRequest<Result<IReadOnlyList<WorkflowSlaDto>>>;
