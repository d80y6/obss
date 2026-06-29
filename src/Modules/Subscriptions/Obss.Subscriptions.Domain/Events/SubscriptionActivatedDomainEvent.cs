using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class SubscriptionActivatedDomainEvent : DomainEvent
{
    public SubscriptionActivatedDomainEvent(
        Guid subscriptionId,
        Guid customerId,
        Guid offerId,
        DateTime startDate)
    {
        SubscriptionId = subscriptionId;
        CustomerId = customerId;
        OfferId = offerId;
        StartDate = startDate;
    }

    public Guid SubscriptionId { get; }
    public Guid CustomerId { get; }
    public Guid OfferId { get; }
    public DateTime StartDate { get; }
}
