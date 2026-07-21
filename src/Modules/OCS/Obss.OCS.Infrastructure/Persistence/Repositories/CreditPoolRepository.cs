using Microsoft.EntityFrameworkCore;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Domain.Entities;
using Obss.OCS.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.OCS.Infrastructure.Persistence.Repositories;

public sealed class CreditPoolRepository : EfRepository<CreditPool>, ICreditPoolRepository
{
    public CreditPoolRepository(OcsDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<CreditPool>> GetActiveBySubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.SubscriptionId == subscriptionId && p.Status == CreditPoolStatus.Active)
            .OrderBy(p => p.ExpiresAt)
            .ToListAsync(cancellationToken);
    }
}
