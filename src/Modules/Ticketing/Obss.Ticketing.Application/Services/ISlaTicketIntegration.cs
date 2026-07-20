using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Services;

public sealed record SlaStatusResult(
    bool SlaBreached,
    TimeSpan? RemainingTime,
    DateTime? SlaDeadline);

public interface ISlaTicketIntegration
{
    Task LinkSlaToTicketAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task<SlaStatusResult> GetSlaStatusAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task EscalateOnSlaBreachAsync(Ticket ticket, CancellationToken cancellationToken = default);
}
