using Obss.SharedKernel.Domain.Events;

namespace Obss.Orders.Application.IntegrationEvents;

public sealed class ProductOrderItemStateChangedIntegrationEvent : IntegrationEvent
{
    public ProductOrderItemStateChangedIntegrationEvent(
        Guid orderId,
        Guid itemId,
        string oldState,
        string newState,
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
    public string OldState { get; }
    public string NewState { get; }
    public string? Reason { get; }
}
