using Microsoft.EntityFrameworkCore;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Billing.Infrastructure.Persistence.Repositories;

public sealed class TaxRuleRepository : EfRepository<TaxRule>, ITaxRuleRepository
{
    private readonly BillingDbContext _billingDbContext;

    public TaxRuleRepository(BillingDbContext context)
        : base(context)
    {
        _billingDbContext = context;
    }

    public async Task<IReadOnlyList<TaxRule>> GetApplicableRulesAsync(string category, string country, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(r => r.TaxCategory == category);
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(r => r.Country == country);
        }

        return await query
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaxExemption>> GetCustomerExemptionsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _billingDbContext.Set<TaxExemption>()
            .Where(e => e.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaxExemption?> GetExemptionAsync(Guid customerId, Guid taxRuleId, CancellationToken cancellationToken = default)
    {
        return await _billingDbContext.Set<TaxExemption>()
            .Where(e => e.CustomerId == customerId && e.TaxRuleId == taxRuleId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddExemptionAsync(TaxExemption exemption, CancellationToken cancellationToken = default)
    {
        await _billingDbContext.Set<TaxExemption>().AddAsync(exemption, cancellationToken);
    }
}
