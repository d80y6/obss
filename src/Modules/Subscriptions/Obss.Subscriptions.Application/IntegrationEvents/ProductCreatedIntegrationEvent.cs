using Obss.SharedKernel.Domain.Events;

namespace Obss.Subscriptions.Application.IntegrationEvents;

public sealed class ProductCreatedIntegrationEvent : IntegrationEvent
{
    public ProductCreatedIntegrationEvent(Guid productId, Guid customerId, Guid? productOfferingId)
    {
        ProductId = productId;
        CustomerId = customerId;
        ProductOfferingId = productOfferingId;
    }

    public Guid ProductId { get; }
    public Guid CustomerId { get; }
    public Guid? ProductOfferingId { get; }
}
