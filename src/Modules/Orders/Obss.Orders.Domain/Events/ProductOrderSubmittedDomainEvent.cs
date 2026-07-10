using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Events;

public sealed record OrderItemData(
    Guid ItemId,
    Guid ProductId,
    Guid OfferId,
    string ProductName,
    string OfferName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

public sealed class ProductOrderSubmittedDomainEvent : DomainEvent
{
    public ProductOrderSubmittedDomainEvent(
        Guid orderId,
        string orderNumber,
        Guid customerId,
        decimal grandTotal,
        string currency,
        List<OrderItemData> orderItems)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
        GrandTotal = grandTotal;
        Currency = currency;
        OrderItems = orderItems;
    }

    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public Guid CustomerId { get; }
    public decimal GrandTotal { get; }
    public string Currency { get; }
    public List<OrderItemData> OrderItems { get; }
}
