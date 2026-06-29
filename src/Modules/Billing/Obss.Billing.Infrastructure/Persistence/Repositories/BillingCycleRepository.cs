using Microsoft.EntityFrameworkCore;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Billing.Infrastructure.Persistence.Repositories;

public sealed class BillingCycleRepository : EfRepository<BillingCycle>, IBillingCycleRepository
{
    public BillingCycleRepository(BillingDbContext context)
        : base(context)
    {
    }

    public async Task<BillingCycle?> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.CustomerId == customerId && c.Status == BillingCycleStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BillingCycle>> GetCyclesDueAsync(DateTime upToDate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c =>
                c.Status == BillingCycleStatus.Active &&
                c.NextBillingDate <= upToDate)
            .OrderBy(c => c.NextBillingDate)
            .ToListAsync(cancellationToken);
    }
}
