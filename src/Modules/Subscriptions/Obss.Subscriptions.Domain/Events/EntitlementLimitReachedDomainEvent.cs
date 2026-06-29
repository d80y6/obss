using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class EntitlementLimitReachedDomainEvent : DomainEvent
{
    public EntitlementLimitReachedDomainEvent(
        Guid subscriptionId,
        string entitlementType,
        decimal currentUsage,
        decimal limit)
    {
        SubscriptionId = subscriptionId;
        EntitlementType = entitlementType;
        CurrentUsage = currentUsage;
        Limit = limit;
    }

    public Guid SubscriptionId { get; }
    public string EntitlementType { get; }
    public decimal CurrentUsage { get; }
    public decimal Limit { get; }
}
