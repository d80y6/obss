using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Domain.Entities;
using Obss.Ticketing.Domain.ValueObjects;

namespace Obss.Ticketing.Infrastructure.Persistence.Repositories;

public sealed class TicketRepository : EfRepository<Ticket>, ITicketRepository
{
    public TicketRepository(TicketDbContext context)
        : base(context)
    {
    }

    public async Task<Ticket?> GetByIdWithDetailsAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
    }

    public async Task<Ticket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetFilteredAsync(
        string? tenantId,
        string? status,
        string? priority,
        string? category,
        Guid? customerId,
        string? assignedTo,
        DateTime? fromDate,
        DateTime? toDate,
        int offset,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
            query = query.Where(t => t.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, true, out var statusVal))
            query = query.Where(t => t.Status == statusVal);

        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TicketPriority>(priority, true, out var priorityVal))
            query = query.Where(t => t.Priority == priorityVal);

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<TicketCategory>(category, true, out var categoryVal))
            query = query.Where(t => t.Category == categoryVal);

        if (customerId.HasValue)
            query = query.Where(t => t.CustomerId == customerId.Value);

        if (!string.IsNullOrWhiteSpace(assignedTo))
            query = query.Where(t => t.AssignedTo == assignedTo);

        if (fromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.CreatedAt <= toDate.Value);

        query = query
            .OrderByDescending(t => t.CreatedAt)
            .Skip(offset)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetOpenTicketsAsync(string? tenantId, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
            query = query.Where(t => t.TenantId == tenantId);

        return await query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> GetNextTicketNumberAsync(CancellationToken cancellationToken = default)
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"TKT-{datePart}-";

        var lastTicket = await DbSet
            .Where(t => t.TicketNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TicketNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastTicket is null)
            return $"{prefix}0001";

        var lastSeq = lastTicket.TicketNumber[prefix.Length..];
        if (int.TryParse(lastSeq, out var seq))
            return $"{prefix}{seq + 1:D4}";

        return $"{prefix}0001";
    }

    public async Task<IReadOnlyList<Ticket>> GetSlaBreachedTicketsAsync(string? tenantId, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(t => t.SlaBreachedAt.HasValue)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
            query = query.Where(t => t.TenantId == tenantId);

        return await query
            .OrderByDescending(t => t.SlaBreachedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsApproachingSlaBreachAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await DbSet
            .Where(t => t.SlaDeadline.HasValue
                && !t.SlaBreachedAt.HasValue
                && t.Status != TicketStatus.Resolved
                && t.Status != TicketStatus.Closed
                && t.SlaDeadline <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(string? tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return await DbSet.CountAsync(cancellationToken);

        return await DbSet.CountAsync(t => t.TenantId == tenantId, cancellationToken);
    }
}
