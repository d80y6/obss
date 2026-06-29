using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class EntitlementUpdatedDomainEvent : DomainEvent
{
    public EntitlementUpdatedDomainEvent(
        Guid subscriptionId,
        string entitlementType,
        decimal previousUsed,
        decimal newUsed)
    {
        SubscriptionId = subscriptionId;
        EntitlementType = entitlementType;
        PreviousUsed = previousUsed;
        NewUsed = newUsed;
    }

    public Guid SubscriptionId { get; }
    public string EntitlementType { get; }
    public decimal PreviousUsed { get; }
    public decimal NewUsed { get; }
}
