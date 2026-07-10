using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Events;

public sealed class ProductOrderApprovedDomainEvent : DomainEvent
{
    public ProductOrderApprovedDomainEvent(Guid orderId, string orderNumber, string approvedBy)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        ApprovedBy = approvedBy;
    }

    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public string ApprovedBy { get; }
}
