using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Events;

public sealed class ProductOrderCompletedDomainEvent : DomainEvent
{
    public ProductOrderCompletedDomainEvent(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}
