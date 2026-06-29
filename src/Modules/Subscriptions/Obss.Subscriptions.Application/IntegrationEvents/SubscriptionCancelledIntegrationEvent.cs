using Obss.SharedKernel.Domain.Events;

namespace Obss.Subscriptions.Application.IntegrationEvents;

public sealed class SubscriptionCancelledIntegrationEvent : IntegrationEvent
{
    public SubscriptionCancelledIntegrationEvent(
        Guid subscriptionId,
        Guid customerId,
        Guid offerId,
        DateTime cancelledAt,
        string tenantId)
    {
        SubscriptionId = subscriptionId;
        CustomerId = customerId;
        OfferId = offerId;
        CancelledAt = cancelledAt;
        TenantId = tenantId;
    }

    public Guid SubscriptionId { get; }
    public Guid CustomerId { get; }
    public Guid OfferId { get; }
    public DateTime CancelledAt { get; }
}
