using Microsoft.EntityFrameworkCore;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.OCS.Infrastructure.Persistence.Repositories;

public sealed class OcsTransactionRepository : EfRepository<OcsTransaction>, IOcsTransactionRepository
{
    public OcsTransactionRepository(OcsDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<OcsTransaction>> GetBySubscriptionAsync(Guid subscriptionId, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.SubscriptionId == subscriptionId)
            .OrderByDescending(t => t.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
