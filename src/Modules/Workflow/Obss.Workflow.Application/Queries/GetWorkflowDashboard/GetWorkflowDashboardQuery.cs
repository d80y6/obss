using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetWorkflowDashboard;

public sealed record GetWorkflowDashboardQuery : IRequest<Result<WorkflowDashboardDto>>;
