using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetSlaBreachedWorkflows;

public sealed class GetSlaBreachedWorkflowsQueryHandler : IRequestHandler<GetSlaBreachedWorkflowsQuery, Result<IReadOnlyList<WorkflowInstanceDto>>>
{
    private readonly IWorkflowInstanceRepository _repository;

    public GetSlaBreachedWorkflowsQueryHandler(IWorkflowInstanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<WorkflowInstanceDto>>> Handle(GetSlaBreachedWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var instances = await _repository.GetAllAsync(cancellationToken);
        var breached = instances
            .Where(i => i.IsSlaBreached)
            .OrderByDescending(i => i.SlaBreachedAt)
            .ToList();

        var result = breached.Adapt<List<WorkflowInstanceDto>>();
        return Result.Success<IReadOnlyList<WorkflowInstanceDto>>(result);
    }
}
