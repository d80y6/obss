using Obss.SharedKernel.Domain.Events;

namespace Obss.Orders.Application.IntegrationEvents;

public sealed class ProductOrderSubmittedIntegrationEvent : IntegrationEvent
{
    public ProductOrderSubmittedIntegrationEvent(
        Guid orderId,
        string orderNumber,
        Guid customerId,
        decimal grandTotal,
        string currency,
        List<ProductOrderItemIntegrationData> items)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
        GrandTotal = grandTotal;
        Currency = currency;
        Items = items;
    }

    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public Guid CustomerId { get; }
    public decimal GrandTotal { get; }
    public string Currency { get; }
    public List<ProductOrderItemIntegrationData> Items { get; }
}

public sealed record ProductOrderItemIntegrationData(
    Guid ProductId,
    Guid OfferId,
    string ProductName,
    string OfferName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
