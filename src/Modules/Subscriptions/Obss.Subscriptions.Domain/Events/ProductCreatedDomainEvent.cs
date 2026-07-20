using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductCreatedDomainEvent : DomainEvent
{
    public ProductCreatedDomainEvent(Guid productId, Guid customerId, Guid? productOfferingId)
    {
        ProductId = productId;
        CustomerId = customerId;
        ProductOfferingId = productOfferingId;
    }

    public Guid ProductId { get; }
    public Guid CustomerId { get; }
    public Guid? ProductOfferingId { get; }
}
