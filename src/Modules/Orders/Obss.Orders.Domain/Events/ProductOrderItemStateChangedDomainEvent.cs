using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Events;

public sealed class ProductOrderItemStateChangedDomainEvent : DomainEvent
{
    public ProductOrderItemStateChangedDomainEvent(
        Guid orderId,
        Guid itemId,
        ProductOrderItemState oldState,
        ProductOrderItemState newState,
        string? reason)
    {
        OrderId = orderId;
        ItemId = itemId;
        OldState = oldState;
        NewState = newState;
        Reason = reason;
    }

    public Guid OrderId { get; }
    public Guid ItemId { get; }
    public ProductOrderItemState OldState { get; }
    public ProductOrderItemState NewState { get; }
    public string? Reason { get; }
}
