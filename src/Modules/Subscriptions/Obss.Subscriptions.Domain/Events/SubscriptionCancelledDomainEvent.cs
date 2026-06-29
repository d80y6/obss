using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class SubscriptionCancelledDomainEvent : DomainEvent
{
    public SubscriptionCancelledDomainEvent(
        Guid subscriptionId,
        Guid customerId,
        DateTime cancelledAt)
    {
        SubscriptionId = subscriptionId;
        CustomerId = customerId;
        CancelledAt = cancelledAt;
    }

    public Guid SubscriptionId { get; }
    public Guid CustomerId { get; }
    public DateTime CancelledAt { get; }
}
