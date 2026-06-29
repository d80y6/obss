using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Domain.Entities;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionRepository : EfRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(SubscriptionDbContext context)
        : base(context)
    {
    }

    public async Task<Subscription?> GetByIdWithDetailsAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.AddOns)
            .Include(s => s.Services)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetFilteredAsync(
        Guid? customerId,
        SubscriptionStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(s => s.CustomerId == customerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.StartDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.StartDate <= toDate.Value);
        }

        query = query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetActiveByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.CustomerId == customerId && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetSubscriptionsDueForRenewalAsync(DateTime upToDate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s =>
                s.Status == SubscriptionStatus.Active &&
                s.RenewalDate.HasValue &&
                s.RenewalDate.Value <= upToDate)
            .OrderBy(s => s.RenewalDate)
            .ToListAsync(cancellationToken);
    }
}
