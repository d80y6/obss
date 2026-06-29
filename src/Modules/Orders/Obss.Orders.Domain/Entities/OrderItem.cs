using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Entities;

public class OrderItem : Entity<Guid>
{
    internal OrderItem(
        Guid id,
        Guid orderId,
        Guid productId,
        Guid offerId,
        string productName,
        string offerName,
        int quantity,
        decimal unitPrice,
        decimal recurringPrice,
        decimal discountAmount,
        decimal taxAmount,
        decimal totalPrice,
        BillingPeriod billingPeriod,
        DateTime startDate,
        DateTime? endDate,
        bool isActive,
        string? serviceType = null)
        : base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        OfferId = offerId;
        ProductName = productName;
        OfferName = offerName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        RecurringPrice = recurringPrice;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        TotalPrice = totalPrice;
        BillingPeriod = billingPeriod;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        ServiceType = serviceType;
    }

    private OrderItem() { }

    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid OfferId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string OfferName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal RecurringPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalPrice { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? ServiceType { get; private set; }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity = quantity;
    }
}
