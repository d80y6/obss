using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetPendingTasks;

public sealed record GetPendingTasksQuery : IRequest<Result<IReadOnlyList<WorkflowTaskDto>>>;
