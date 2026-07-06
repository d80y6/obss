using Microsoft.EntityFrameworkCore;
using Obss.EventManagement.Application.Abstractions;
using Obss.EventManagement.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.EventManagement.Infrastructure.Persistence.Repositories;

public sealed class EventSubscriptionRepository : EfRepository<EventSubscription>, IEventSubscriptionRepository
{
    public EventSubscriptionRepository(EventDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<EventSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.Status == "active")
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EventSubscription>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.Status == "active" && s.Filters.Any(f => f.EventType == eventType))
            .ToListAsync(cancellationToken);
    }
}
