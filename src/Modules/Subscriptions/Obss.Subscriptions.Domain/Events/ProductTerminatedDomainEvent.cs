using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Events;

public sealed class ProductTerminatedDomainEvent : DomainEvent
{
    public ProductTerminatedDomainEvent(Guid productId)
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}