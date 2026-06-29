using Obss.SharedKernel.Application.Abstractions;
using Obss.Subscriptions.Domain.Entities;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.Abstractions;

public interface ISubscriptionEntitlementRepository : IRepository<SubscriptionEntitlement>
{
    Task<IReadOnlyList<SubscriptionEntitlement>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<SubscriptionEntitlement?> GetBySubscriptionAndTypeAsync(Guid subscriptionId, EntitlementType entitlementType, CancellationToken cancellationToken = default);
    Task DeleteBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
