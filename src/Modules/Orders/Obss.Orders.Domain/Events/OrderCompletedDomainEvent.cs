using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Events;

public sealed class OrderCompletedDomainEvent : DomainEvent
{
    public OrderCompletedDomainEvent(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}
