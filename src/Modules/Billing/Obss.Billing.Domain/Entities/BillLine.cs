using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Entities;

public class BillLine : Entity<Guid>
{
    private BillLine() { }

    private BillLine(
        Guid id,
        Guid billId,
        LineType lineType,
        string description,
        Guid? subscriptionId,
        Guid? productId,
        Guid? offerId,
        int quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxRate,
        string currency,
        DateTime lineDate,
        string? reference)
        : base(id)
    {
        BillId = billId;
        LineType = lineType;
        Description = description;
        SubscriptionId = subscriptionId;
        ProductId = productId;
        OfferId = offerId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        TaxRate = taxRate;
        Currency = currency;
        LineDate = lineDate;
        Reference = reference;
        CalculateTotal();
    }

    public Guid BillId { get; private set; }
    public LineType LineType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? SubscriptionId { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? OfferId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime LineDate { get; private set; }
    public string? Reference { get; private set; }

    public static BillLine CreateRecurring(
        Guid billId,
        string description,
        Guid subscriptionId,
        Guid? productId,
        Guid? offerId,
        int quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxRate,
        string currency,
        DateTime lineDate)
    {
        return new BillLine(
            Guid.NewGuid(),
            billId,
            LineType.Recurring,
            description,
            subscriptionId,
            productId,
            offerId,
            quantity,
            unitPrice,
            discountAmount,
            taxRate,
            currency,
            lineDate,
            null);
    }

    public static BillLine CreateUsage(
        Guid billId,
        string description,
        Guid subscriptionId,
        Guid? productId,
        Guid? offerId,
        int quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxRate,
        string currency,
        DateTime lineDate,
        string usageReference)
    {
        return new BillLine(
            Guid.NewGuid(),
            billId,
            LineType.Usage,
            description,
            subscriptionId,
            productId,
            offerId,
            quantity,
            unitPrice,
            discountAmount,
            taxRate,
            currency,
            lineDate,
            usageReference);
    }

    public static BillLine CreateAdjustment(
        Guid billId,
        string description,
        decimal amount,
        string currency,
        DateTime lineDate)
    {
        return new BillLine(
            Guid.NewGuid(),
            billId,
            LineType.Adjustment,
            description,
            null,
            null,
            null,
            1,
            amount,
            0,
            0,
            currency,
            lineDate,
            null);
    }

    public static BillLine CreateTaxLine(
        Guid billId,
        string description,
        decimal amount,
        decimal taxRate,
        string currency,
        DateTime lineDate)
    {
        return new BillLine(
            Guid.NewGuid(),
            billId,
            LineType.Tax,
            description,
            null,
            null,
            null,
            1,
            amount,
            0,
            taxRate,
            currency,
            lineDate,
            null);
    }

    public void CalculateTotal()
    {
        var subtotal = UnitPrice * Quantity;
        var afterDiscount = subtotal - DiscountAmount;
        TaxAmount = afterDiscount * TaxRate;
        TotalAmount = afterDiscount + TaxAmount;
    }
}
