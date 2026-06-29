using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;

namespace Obss.Workflow.Application.Queries.GetPendingTasks;

public sealed class GetPendingTasksQueryHandler : IRequestHandler<GetPendingTasksQuery, Result<IReadOnlyList<WorkflowTaskDto>>>
{
    private readonly IWorkflowInstanceRepository _repository;

    public GetPendingTasksQueryHandler(IWorkflowInstanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<WorkflowTaskDto>>> Handle(GetPendingTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _repository.GetPendingTasksAsync(cancellationToken);
        var result = tasks.Adapt<List<WorkflowTaskDto>>();
        return Result.Success<IReadOnlyList<WorkflowTaskDto>>(result);
    }
}
