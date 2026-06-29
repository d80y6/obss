using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Events;

public sealed class OrderCancelledDomainEvent : DomainEvent
{
    public OrderCancelledDomainEvent(Guid orderId, string reason)
    {
        OrderId = orderId;
        Reason = reason;
    }

    public Guid OrderId { get; }
    public string Reason { get; }
}
