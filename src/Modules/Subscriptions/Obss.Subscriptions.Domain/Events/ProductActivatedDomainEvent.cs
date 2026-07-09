using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductActivatedDomainEvent : DomainEvent
{
    public ProductActivatedDomainEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}