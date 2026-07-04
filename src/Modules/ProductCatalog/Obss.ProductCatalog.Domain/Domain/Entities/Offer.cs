using Obss.ProductCatalog.Domain.Domain.Events;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class Offer : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<OfferPricing> _offerPricings = [];
    private readonly List<OfferDiscount> _discounts = [];
    private readonly List<ProductOfferingTerm> _terms = [];
    private readonly List<BundledProductOffering> _bundledOfferings = [];

    private Offer() { }

    private Offer(
        Guid id,
        string tenantId,
        string name,
        string? description,
        OfferType offerType,
        bool isContract,
        int? contractDurationMonths,
        BillingPeriod? billingPeriod,
        bool taxInclusive,
        int sortOrder,
        DateTime? validFrom,
        DateTime? validTo)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        OfferType = offerType;
        IsActive = true;
        IsContract = isContract;
        ContractDurationMonths = contractDurationMonths;
        BillingPeriod = billingPeriod;
        TaxInclusive = taxInclusive;
        SortOrder = sortOrder;
        ValidFrom = validFrom;
        ValidTo = validTo;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OfferCreatedDomainEvent(id, tenantId, name, offerType));
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public OfferType OfferType { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsContract { get; private set; }
    public int? ContractDurationMonths { get; private set; }
    public BillingPeriod? BillingPeriod { get; private set; }
    public bool TaxInclusive { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<OfferPricing> OfferPricings => _offerPricings.AsReadOnly();
    public IReadOnlyCollection<OfferDiscount> Discounts => _discounts.AsReadOnly();
    public IReadOnlyCollection<ProductOfferingTerm> Terms => _terms.AsReadOnly();
    public IReadOnlyCollection<BundledProductOffering> BundledOfferings => _bundledOfferings.AsReadOnly();

    public static Offer Create(
        string tenantId,
        string name,
        string? description,
        OfferType offerType,
        bool isContract,
        int? contractDurationMonths,
        BillingPeriod? billingPeriod,
        bool taxInclusive,
        int sortOrder,
        DateTime? validFrom,
        DateTime? validTo)
    {
        return new Offer(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            offerType,
            isContract,
            contractDurationMonths,
            billingPeriod,
            taxInclusive,
            sortOrder,
            validFrom,
            validTo);
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetValidityPeriod(DateTime? validFrom, DateTime? validTo)
    {
        ValidFrom = validFrom;
        ValidTo = validTo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddPricing(OfferPricing pricing)
    {
        _offerPricings.Add(pricing);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OfferPricingChangedDomainEvent(Id, pricing.Id, pricing.PricingType));
    }

    public void RemovePricing(OfferPricing pricing)
    {
        _offerPricings.Remove(pricing);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddDiscount(OfferDiscount discount)
    {
        _discounts.Add(discount);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveDiscount(OfferDiscount discount)
    {
        _discounts.Remove(discount);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTerm(ProductOfferingTerm term)
    {
        _terms.Add(term);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveTerm(ProductOfferingTerm term)
    {
        _terms.Remove(term);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddBundledOffering(BundledProductOffering bundledOffering)
    {
        _bundledOfferings.Add(bundledOffering);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveBundledOffering(BundledProductOffering bundledOffering)
    {
        _bundledOfferings.Remove(bundledOffering);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string name,
        string? description,
        bool isContract,
        int? contractDurationMonths,
        BillingPeriod? billingPeriod,
        bool taxInclusive,
        int sortOrder)
    {
        Name = name;
        Description = description;
        IsContract = isContract;
        ContractDurationMonths = contractDurationMonths;
        BillingPeriod = billingPeriod;
        TaxInclusive = taxInclusive;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }
}
