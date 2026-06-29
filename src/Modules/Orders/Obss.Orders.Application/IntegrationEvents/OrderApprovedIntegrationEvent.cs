using Obss.SharedKernel.Domain.Events;

namespace Obss.Orders.Application.IntegrationEvents;

public sealed class OrderApprovedIntegrationEvent : IntegrationEvent
{
    public OrderApprovedIntegrationEvent(
        Guid orderId,
        string orderNumber,
        string approvedBy,
        DateTime approvedAt)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        ApprovedBy = approvedBy;
        ApprovedAt = approvedAt;
    }

    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public string ApprovedBy { get; }
    public DateTime ApprovedAt { get; }
}
