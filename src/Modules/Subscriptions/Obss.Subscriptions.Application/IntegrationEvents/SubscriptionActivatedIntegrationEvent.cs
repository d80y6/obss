using Obss.SharedKernel.Domain.Events;

namespace Obss.Subscriptions.Application.IntegrationEvents;

public sealed class SubscriptionActivatedIntegrationEvent : IntegrationEvent
{
    public SubscriptionActivatedIntegrationEvent(
        Guid subscriptionId,
        Guid customerId,
        Guid offerId,
        DateTime activationDate,
        string tenantId)
    {
        SubscriptionId = subscriptionId;
        CustomerId = customerId;
        OfferId = offerId;
        ActivationDate = activationDate;
        TenantId = tenantId;
    }

    public Guid SubscriptionId { get; }
    public Guid CustomerId { get; }
    public Guid OfferId { get; }
    public DateTime ActivationDate { get; }
}
