using Obss.Provisioning.Domain.Entities;

namespace Obss.Provisioning.Application.Abstractions;

public enum ProvisioningState
{
    Success,
    Failed,
    BlockedNeedsOperator,
    PendingVendorConfirmation
}

public sealed record ProvisioningResult
{
    public bool Success { get; init; }
    public ProvisioningState State { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ResultData { get; init; }
    public TimeSpan Duration { get; init; }

    public static ProvisioningResult Ok(string? resultData = null, TimeSpan? duration = null)
        => new() { Success = true, State = ProvisioningState.Success, ResultData = resultData, Duration = duration ?? TimeSpan.Zero };

    public static ProvisioningResult Fail(string error, TimeSpan? duration = null)
        => new() { Success = false, State = ProvisioningState.Failed, ErrorMessage = error, Duration = duration ?? TimeSpan.Zero };

    public static ProvisioningResult Blocked(string reason, TimeSpan? duration = null)
        => new() { Success = false, State = ProvisioningState.BlockedNeedsOperator, ErrorMessage = reason, Duration = duration ?? TimeSpan.Zero };

    public static ProvisioningResult PendingConfirmation(string? resultData = null, TimeSpan? duration = null)
        => new() { Success = true, State = ProvisioningState.PendingVendorConfirmation, ResultData = resultData, Duration = duration ?? TimeSpan.Zero };
}

public interface IProvisioningAdapter
{
    string AdapterName { get; }
    Task<ProvisioningResult> ExecuteAsync(ProvisioningTask task, CancellationToken cancellationToken);
    Task<ProvisioningResult> CompensateAsync(ProvisioningTask task, CancellationToken cancellationToken);
}
