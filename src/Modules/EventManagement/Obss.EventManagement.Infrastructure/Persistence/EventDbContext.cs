using Microsoft.EntityFrameworkCore;
using Obss.EventManagement.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.EventManagement.Infrastructure.Persistence;

public class EventDbContext : EfDbContext
{
    public EventDbContext(
        DbContextOptions<EventDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<EventSubscription> EventSubscriptions => Set<EventSubscription>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
