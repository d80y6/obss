using Obss.Provisioning.Domain.Entities;

namespace Obss.Provisioning.Application.Services;

public sealed record TaskExecutionResult(
    Guid TaskId,
    bool Success,
    string? ErrorMessage,
    TimeSpan Duration);

public sealed record CoordinatorProgress(
    Guid JobId,
    int CompletedTasks,
    int TotalTasks,
    string Status,
    string? CurrentTaskType,
    string? ErrorMessage);

public interface IProvisioningJobCoordinator
{
    Task<CoordinatorProgress> ExecuteJobAsync(ProvisioningJob job, CancellationToken ct);
    Task<CoordinatorProgress> GetProgressAsync(Guid jobId, CancellationToken ct);
}
