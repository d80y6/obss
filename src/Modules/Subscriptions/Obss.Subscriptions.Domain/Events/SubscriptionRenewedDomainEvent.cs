using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class SubscriptionRenewedDomainEvent : DomainEvent
{
    public SubscriptionRenewedDomainEvent(
        Guid subscriptionId,
        DateTime renewalDate,
        decimal price)
    {
        SubscriptionId = subscriptionId;
        RenewalDate = renewalDate;
        Price = price;
    }

    public Guid SubscriptionId { get; }
    public DateTime RenewalDate { get; }
    public decimal Price { get; }
}
