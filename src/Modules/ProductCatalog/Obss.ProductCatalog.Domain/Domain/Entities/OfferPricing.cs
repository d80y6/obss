using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class OfferPricing : Entity<Guid>
{
    private OfferPricing() { }

    public OfferPricing(
        Guid id,
        Guid offerId,
        PricingType pricingType,
        string currency,
        decimal recurringPrice,
        decimal oneTimePrice,
        decimal usagePrice,
        string? unitOfMeasure,
        int? minQuantity,
        int? maxQuantity,
        bool isActive)
        : base(id)
    {
        OfferId = offerId;
        PricingType = pricingType;
        Currency = currency;
        RecurringPrice = recurringPrice;
        OneTimePrice = oneTimePrice;
        UsagePrice = usagePrice;
        UnitOfMeasure = unitOfMeasure;
        MinQuantity = minQuantity;
        MaxQuantity = maxQuantity;
        IsActive = isActive;
    }

    public Guid OfferId { get; private set; }
    public PricingType PricingType { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal RecurringPrice { get; private set; }
    public decimal OneTimePrice { get; private set; }
    public decimal UsagePrice { get; private set; }
    public string? UnitOfMeasure { get; private set; }
    public int? MinQuantity { get; private set; }
    public int? MaxQuantity { get; private set; }
    public bool IsActive { get; private set; }

    public void UpdatePricing(
        PricingType pricingType,
        string currency,
        decimal recurringPrice,
        decimal oneTimePrice,
        decimal usagePrice,
        string? unitOfMeasure,
        int? minQuantity,
        int? maxQuantity,
        bool isActive)
    {
        PricingType = pricingType;
        Currency = currency;
        RecurringPrice = recurringPrice;
        OneTimePrice = oneTimePrice;
        UsagePrice = usagePrice;
        UnitOfMeasure = unitOfMeasure;
        MinQuantity = minQuantity;
        MaxQuantity = maxQuantity;
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
