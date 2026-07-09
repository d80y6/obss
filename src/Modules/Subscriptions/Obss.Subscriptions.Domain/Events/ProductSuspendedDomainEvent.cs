using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductSuspendedDomainEvent : DomainEvent
{
    public ProductSuspendedDomainEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}