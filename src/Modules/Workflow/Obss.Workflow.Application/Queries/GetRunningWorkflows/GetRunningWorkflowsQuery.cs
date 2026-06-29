using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetRunningWorkflows;

public sealed record GetRunningWorkflowsQuery : IRequest<Result<IReadOnlyList<RunningWorkflowDto>>>;
