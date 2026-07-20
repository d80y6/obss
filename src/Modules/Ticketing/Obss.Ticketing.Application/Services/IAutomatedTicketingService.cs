using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Services;

public sealed record TicketCreationResult(
    Guid TicketId,
    string TicketNumber,
    bool Created);

public interface IAutomatedTicketingService
{
    Task<TicketCreationResult> CreateTicketFromAlarmAsync(Alarm alarm, CancellationToken cancellationToken = default);
    Task UpdateTicketOnAlarmClearAsync(Alarm alarm, CancellationToken cancellationToken = default);
}
