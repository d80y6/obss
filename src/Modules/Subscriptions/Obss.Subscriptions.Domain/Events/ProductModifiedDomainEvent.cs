using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductModifiedDomainEvent : DomainEvent
{
    public ProductModifiedDomainEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
