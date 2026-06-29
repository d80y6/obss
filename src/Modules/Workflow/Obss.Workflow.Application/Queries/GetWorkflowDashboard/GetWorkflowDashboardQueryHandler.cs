using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;
using Obss.Workflow.Domain.ValueObjects;

namespace Obss.Workflow.Application.Queries.GetWorkflowDashboard;

public sealed class GetWorkflowDashboardQueryHandler : IRequestHandler<GetWorkflowDashboardQuery, Result<WorkflowDashboardDto>>
{
    private readonly IWorkflowInstanceRepository _instanceRepository;

    public GetWorkflowDashboardQueryHandler(IWorkflowInstanceRepository instanceRepository)
    {
        _instanceRepository = instanceRepository;
    }

    public async Task<Result<WorkflowDashboardDto>> Handle(GetWorkflowDashboardQuery request, CancellationToken cancellationToken)
    {
        var allInstances = await _instanceRepository.GetAllAsync(cancellationToken);

        var totalRunning = allInstances.Count(i => i.Status == InstanceStatus.Running);
        var totalCompleted = allInstances.Count(i => i.Status == InstanceStatus.Completed);
        var totalFailed = allInstances.Count(i => i.Status == InstanceStatus.Failed);
        var totalPending = allInstances.Count(i => i.Status == InstanceStatus.Pending);
        var totalPaused = allInstances.Count(i => i.Status == InstanceStatus.Paused);
        var totalCancelled = allInstances.Count(i => i.Status == InstanceStatus.Cancelled);

        var totalFinished = totalCompleted + totalFailed;
        var failureRate = totalFinished > 0 ? (double)totalFailed / totalFinished : 0;

        // Average duration for completed workflows
        var completedInstances = allInstances
            .Where(i => i.Status == InstanceStatus.Completed && i.StartedAt.HasValue && i.CompletedAt.HasValue)
            .ToList();

        var avgDurationSeconds = completedInstances.Count > 0
            ? completedInstances.Average(i => (i.CompletedAt!.Value - i.StartedAt!.Value).TotalSeconds)
            : 0;

        // SLA compliance rate
        var instancesWithSla = allInstances
            .Where(i => i.SlaDeadline.HasValue)
            .ToList();

        var slaCompliantCount = instancesWithSla.Count(i => !i.IsSlaBreached && i.Status == InstanceStatus.Completed);
        var slaComplianceRate = instancesWithSla.Count > 0
            ? (double)slaCompliantCount / instancesWithSla.Count
            : 1.0;

        var recentCompleted = allInstances
            .Where(i => i.Status == InstanceStatus.Completed && i.CompletedAt.HasValue)
            .OrderByDescending(i => i.CompletedAt)
            .Take(10)
            .Select(i => new RecentWorkflowDto(
                i.Id,
                i.WorkflowDefinitionName,
                i.TriggerEntityType,
                i.CompletedAt,
                i.Status))
            .ToList();

        var dashboard = new WorkflowDashboardDto(
            totalRunning,
            totalCompleted,
            totalFailed,
            totalPending,
            totalPaused,
            totalCancelled,
            avgDurationSeconds,
            failureRate,
            slaComplianceRate,
            recentCompleted);

        return Result.Success(dashboard);
    }
}
