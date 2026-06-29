using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Infrastructure.Persistence;

public class SubscriptionDbContext : EfDbContext
{
    public SubscriptionDbContext(
        DbContextOptions<SubscriptionDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionAddOn> SubscriptionAddOns => Set<SubscriptionAddOn>();
    public DbSet<SubscriptionService> SubscriptionServices => Set<SubscriptionService>();
    public DbSet<SubscriptionEntitlement> SubscriptionEntitlements => Set<SubscriptionEntitlement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubscriptionDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
