using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class SubscriptionSuspendedDomainEvent : DomainEvent
{
    public SubscriptionSuspendedDomainEvent(
        Guid subscriptionId,
        string reason)
    {
        SubscriptionId = subscriptionId;
        Reason = reason;
    }

    public Guid SubscriptionId { get; }
    public string Reason { get; }
}
