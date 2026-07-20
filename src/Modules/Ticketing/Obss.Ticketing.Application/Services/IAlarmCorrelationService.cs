using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Services;

public sealed record CorrelationResult(
    string? RootCauseAlarmId,
    IReadOnlyList<Guid> CorrelatedAlarmIds,
    bool IsRootCause,
    string? CorrelationRule);

public interface IAlarmCorrelationService
{
    Task<CorrelationResult> CorrelateAsync(Alarm alarm, CancellationToken cancellationToken = default);
    Task<bool> IsSuppressedByMaintenanceAsync(Alarm alarm, CancellationToken cancellationToken = default);
    Task EscalateIfNeededAsync(Alarm alarm, CancellationToken cancellationToken = default);
}
