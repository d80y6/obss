using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetWorkflowInstances;

public sealed record GetWorkflowInstancesQuery(
    string? Status,
    string? EntityType,
    Guid? EntityId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<WorkflowInstanceDto>>>;
