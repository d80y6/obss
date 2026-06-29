using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Queries.GetWorkflowSlaStatus;

public sealed class GetWorkflowSlaStatusQueryHandler : IRequestHandler<GetWorkflowSlaStatusQuery, Result<SlaStatusDto>>
{
    private readonly IWorkflowInstanceRepository _repository;

    public GetWorkflowSlaStatusQueryHandler(IWorkflowInstanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SlaStatusDto>> Handle(GetWorkflowSlaStatusQuery request, CancellationToken cancellationToken)
    {
        var instance = await _repository.GetByIdAsync(request.WorkflowInstanceId, cancellationToken);
        if (instance is null)
            return Result.Failure<SlaStatusDto>(Error.NotFound(nameof(WorkflowInstance), request.WorkflowInstanceId));

        TimeSpan? remaining = null;

        if (instance.SlaDeadline.HasValue && !instance.IsSlaBreached)
        {
            remaining = instance.SlaDeadline.Value - DateTime.UtcNow;
            if (remaining < TimeSpan.Zero)
                remaining = TimeSpan.Zero;
        }

        var dto = new SlaStatusDto(
            instance.Id,
            instance.SlaDeadline,
            instance.SlaBreachedAt,
            instance.IsSlaBreached,
            remaining,
            instance.Status.ToString());

        return Result.Success(dto);
    }
}
