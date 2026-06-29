using Obss.SharedKernel.Domain.Events;

namespace Obss.Orders.Application.IntegrationEvents;

public sealed class OrderFulfillmentStartedIntegrationEvent : IntegrationEvent
{
    public OrderFulfillmentStartedIntegrationEvent(
        Guid orderId,
        Guid fulfillmentId,
        string status)
    {
        OrderId = orderId;
        FulfillmentId = fulfillmentId;
        Status = status;
    }

    public Guid OrderId { get; }
    public Guid FulfillmentId { get; }
    public string Status { get; }
}
