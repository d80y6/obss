using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Application.Queries.GetRunningWorkflows;

public sealed class GetRunningWorkflowsQueryHandler : IRequestHandler<GetRunningWorkflowsQuery, Result<IReadOnlyList<RunningWorkflowDto>>>
{
    private readonly IWorkflowInstanceRepository _repository;

    public GetRunningWorkflowsQueryHandler(IWorkflowInstanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<RunningWorkflowDto>>> Handle(GetRunningWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var allInstances = await _repository.GetAllAsync(cancellationToken);
        var running = allInstances
            .Where(i => i.Status == InstanceStatus.Running)
            .OrderByDescending(i => i.StartedAt)
            .Select(i => new RunningWorkflowDto(
                i.Id,
                i.WorkflowDefinitionId,
                i.WorkflowDefinitionName,
                i.TriggerEntityType,
                i.TriggerEntityId,
                i.StartedAt ?? DateTime.UtcNow,
                i.SlaDeadline,
                i.IsSlaBreached,
                i.SlaBreachedAt,
                i.Tasks.Count,
                i.Tasks.Count(t => t.Status == WorkflowTaskStatus.Completed)))
            .ToList();

        return Result.Success<IReadOnlyList<RunningWorkflowDto>>(running);
    }
}
