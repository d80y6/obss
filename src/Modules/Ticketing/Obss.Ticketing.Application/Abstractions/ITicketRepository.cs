using Obss.SharedKernel.Application.Abstractions;
using Obss.Ticketing.Domain.Entities;
using Obss.Ticketing.Domain.ValueObjects;

namespace Obss.Ticketing.Application.Abstractions;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<Ticket?> GetByIdWithDetailsAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetFilteredAsync(
        string? tenantId,
        string? status,
        string? priority,
        string? category,
        Guid? customerId,
        string? assignedTo,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetOpenTicketsAsync(string? tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetSlaBreachedTicketsAsync(string? tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetTicketsApproachingSlaBreachAsync(CancellationToken cancellationToken = default);
    Task<string> GetNextTicketNumberAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(string? tenantId, CancellationToken cancellationToken = default);
}
