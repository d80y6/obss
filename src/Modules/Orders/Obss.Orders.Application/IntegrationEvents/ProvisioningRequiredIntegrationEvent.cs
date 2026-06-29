using Obss.SharedKernel.Domain.Events;

namespace Obss.Orders.Application.IntegrationEvents;

public sealed class ProvisioningRequiredIntegrationEvent : IntegrationEvent
{
    public ProvisioningRequiredIntegrationEvent(
        Guid orderId,
        Guid orderItemId,
        Guid customerId,
        string serviceType,
        string action)
    {
        OrderId = orderId;
        OrderItemId = orderItemId;
        CustomerId = customerId;
        ServiceType = serviceType;
        Action = action;
    }

    public Guid OrderId { get; }
    public Guid OrderItemId { get; }
    public Guid CustomerId { get; }
    public string ServiceType { get; }
    public string Action { get; }
}
