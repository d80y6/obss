using Microsoft.EntityFrameworkCore;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.AAA.Infrastructure.Persistence.Repositories;

public sealed class AaaAuditLogRepository : EfRepository<AaaAuditLog>, IAaaAuditLogRepository
{
    public AaaAuditLogRepository(AaaDbContext context) : base(context)
    {
    }

    public async Task<(IReadOnlyList<AaaAuditLog> Items, int TotalCount)> GetPaginatedAsync(
        int offset,
        int limit,
        string? eventType = null,
        string? username = null,
        Guid? nasId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(l => l.EventType == eventType);
        if (!string.IsNullOrEmpty(username))
            query = query.Where(l => l.Username != null && l.Username == username);
        if (nasId.HasValue)
            query = query.Where(l => l.NasId == nasId.Value);
        if (dateFrom.HasValue)
            query = query.Where(l => l.Timestamp >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(l => l.Timestamp <= dateTo.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<int> CountByDateRangeAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(
            l => l.Timestamp >= dateFrom && l.Timestamp <= dateTo,
            cancellationToken);
    }
}
