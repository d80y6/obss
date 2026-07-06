using Microsoft.EntityFrameworkCore;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Rating.Infrastructure.Persistence.Repositories;

public sealed class UsageRecordRepository : EfRepository<UsageRecord>, IUsageRecordRepository
{
    public UsageRecordRepository(RatingDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<UsageRecord>> GetUnratedRecordsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.Status == UsageStatus.Unrated)
            .OrderBy(r => r.RecordedAt)
            .Take(500)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UsageRecord>> GetBySubscriptionAsync(
        Guid subscriptionId,
        DateTime? from,
        DateTime? to,
        int offset = 0,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(r => r.SubscriptionId == subscriptionId)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(r => r.StartTime >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.EndTime <= to.Value);

        return await query
            .OrderByDescending(r => r.RecordedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UsageRecord>> GetByDateRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.StartTime >= from && r.EndTime <= to)
            .OrderBy(r => r.RecordedAt)
            .ToListAsync(cancellationToken);
    }
}
