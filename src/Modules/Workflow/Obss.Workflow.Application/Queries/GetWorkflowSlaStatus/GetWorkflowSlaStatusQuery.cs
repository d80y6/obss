using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Workflow.Application.Queries.GetWorkflowSlaStatus;

public sealed record GetWorkflowSlaStatusQuery(Guid WorkflowInstanceId) : IRequest<Result<SlaStatusDto>>;

public sealed record SlaStatusDto(
    Guid WorkflowInstanceId,
    DateTime? SlaDeadline,
    DateTime? SlaBreachedAt,
    bool IsSlaBreached,
    TimeSpan? RemainingTime,
    string Status);
