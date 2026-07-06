using Obss.EventManagement.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.EventManagement.Application.Abstractions;

public interface IEventSubscriptionRepository : IRepository<EventSubscription>
{
    Task<IReadOnlyList<EventSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventSubscription>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default);
}
