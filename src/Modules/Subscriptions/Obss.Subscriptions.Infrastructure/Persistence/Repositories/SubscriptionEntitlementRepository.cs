using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Domain.Entities;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionEntitlementRepository : EfRepository<SubscriptionEntitlement>, ISubscriptionEntitlementRepository
{
    public SubscriptionEntitlementRepository(SubscriptionDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<SubscriptionEntitlement>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.SubscriptionId == subscriptionId)
            .OrderBy(e => e.EntitlementType)
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionEntitlement?> GetBySubscriptionAndTypeAsync(Guid subscriptionId, EntitlementType entitlementType, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(
                e => e.SubscriptionId == subscriptionId && e.EntitlementType == entitlementType,
                cancellationToken);
    }

    public async Task DeleteBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var entitlements = await DbSet
            .Where(e => e.SubscriptionId == subscriptionId)
            .ToListAsync(cancellationToken);

        DbSet.RemoveRange(entitlements);
    }
}
