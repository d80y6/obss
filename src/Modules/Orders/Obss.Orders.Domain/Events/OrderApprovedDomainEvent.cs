using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Events;

public sealed class OrderApprovedDomainEvent : DomainEvent
{
    public OrderApprovedDomainEvent(Guid orderId, string orderNumber, string approvedBy)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        ApprovedBy = approvedBy;
    }

    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public string ApprovedBy { get; }
}
