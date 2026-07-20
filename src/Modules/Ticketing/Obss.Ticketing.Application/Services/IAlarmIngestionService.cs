using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Services;

public sealed record AlarmIngestionResult(
    Guid AlarmId,
    bool IsDuplicate,
    bool Suppressed,
    Guid? TicketId,
    string? Error);

public interface IAlarmIngestionService
{
    Task<AlarmIngestionResult> IngestAlarmAsync(Alarm alarm, CancellationToken cancellationToken = default);
}
