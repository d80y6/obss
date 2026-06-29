using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetSlaBreachedWorkflows;

public sealed record GetSlaBreachedWorkflowsQuery : IRequest<Result<IReadOnlyList<WorkflowInstanceDto>>>;
