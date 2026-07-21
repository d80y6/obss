using Microsoft.EntityFrameworkCore;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.OCS.Infrastructure.Persistence.Repositories;

public sealed class BalanceRepository : EfRepository<Balance>, IBalanceRepository
{
    public BalanceRepository(OcsDbContext context) : base(context)
    {
    }

    public async Task<Balance?> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(b => b.SubscriptionId == subscriptionId, cancellationToken);
    }
}
