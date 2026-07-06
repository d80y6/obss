using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetWorkflowInstances;

public sealed class GetWorkflowInstancesQueryHandler : IRequestHandler<GetWorkflowInstancesQuery, Result<IReadOnlyList<WorkflowInstanceDto>>>
{
    private readonly IWorkflowInstanceRepository _repository;

    public GetWorkflowInstancesQueryHandler(IWorkflowInstanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<WorkflowInstanceDto>>> Handle(GetWorkflowInstancesQuery request, CancellationToken cancellationToken)
    {
        var instances = await _repository.GetFilteredAsync(
            request.Status,
            request.EntityType,
            request.EntityId,
            request.Offset,
            request.Limit,
            cancellationToken);

        var result = instances.Adapt<List<WorkflowInstanceDto>>();
        return Result.Success<IReadOnlyList<WorkflowInstanceDto>>(result);
    }
}
