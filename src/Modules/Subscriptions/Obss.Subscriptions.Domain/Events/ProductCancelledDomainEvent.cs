using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductCancelledDomainEvent : DomainEvent
{
    public ProductCancelledDomainEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
