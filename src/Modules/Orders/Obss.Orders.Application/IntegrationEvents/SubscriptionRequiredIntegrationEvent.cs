using Obss.SharedKernel.Domain.Events;

namespace Obss.Orders.Application.IntegrationEvents;

public sealed class SubscriptionRequiredIntegrationEvent : IntegrationEvent
{
    public SubscriptionRequiredIntegrationEvent(
        Guid customerId,
        string customerName,
        Guid orderId,
        Guid orderItemId,
        Guid productId,
        Guid offerId,
        string offerName,
        string billingPeriod,
        string currency,
        decimal price,
        int quantity)
    {
        CustomerId = customerId;
        CustomerName = customerName;
        OrderId = orderId;
        OrderItemId = orderItemId;
        ProductId = productId;
        OfferId = offerId;
        OfferName = offerName;
        BillingPeriod = billingPeriod;
        Currency = currency;
        Price = price;
        Quantity = quantity;
    }

    public Guid CustomerId { get; }
    public string CustomerName { get; }
    public Guid OrderId { get; }
    public Guid OrderItemId { get; }
    public Guid ProductId { get; }
    public Guid OfferId { get; }
    public string OfferName { get; }
    public string BillingPeriod { get; }
    public string Currency { get; }
    public decimal Price { get; }
    public int Quantity { get; }
}
