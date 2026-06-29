using Microsoft.EntityFrameworkCore;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Billing.Infrastructure.Persistence;

public class BillingDbContext : EfDbContext
{
    public BillingDbContext(
        DbContextOptions<BillingDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillLine> BillLines => Set<BillLine>();
    public DbSet<BillingCycle> BillingCycles => Set<BillingCycle>();
    public DbSet<TaxRule> TaxRules => Set<TaxRule>();
    public DbSet<TaxExemption> TaxExemptions => Set<TaxExemption>();
    public DbSet<BillingJob> BillingJobs => Set<BillingJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
