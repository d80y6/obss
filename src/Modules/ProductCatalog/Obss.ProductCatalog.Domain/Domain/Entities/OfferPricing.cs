using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class OfferPricing : Entity<Guid>
{
    private readonly List<PriceRange> _priceRanges = [];

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
        bool isActive,
        string? name = null,
        string? description = null,
        PriceApplicationType? priceApplicationType = null)
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
        Name = name;
        Description = description;
        PriceApplicationType = priceApplicationType;
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
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public PriceApplicationType? PriceApplicationType { get; private set; }

    public IReadOnlyCollection<PriceRange> PriceRanges => _priceRanges.AsReadOnly();

    public void UpdatePricing(
        PricingType pricingType,
        string currency,
        decimal recurringPrice,
        decimal oneTimePrice,
        decimal usagePrice,
        string? unitOfMeasure,
        int? minQuantity,
        int? maxQuantity,
        bool isActive,
        string? name = null,
        string? description = null,
        PriceApplicationType? priceApplicationType = null)
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
        Name = name;
        Description = description;
        PriceApplicationType = priceApplicationType;
    }

    public void AddPriceRange(PriceRange range)
    {
        _priceRanges.Add(range);
    }

    public void RemovePriceRange(PriceRange range)
    {
        _priceRanges.Remove(range);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
