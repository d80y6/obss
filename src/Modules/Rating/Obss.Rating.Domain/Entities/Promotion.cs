using Obss.Rating.Domain.Events;
using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Rating.Domain.Entities;

public class Promotion : AggregateRoot<Guid>
{
    private readonly List<PromotionRule> _rules = [];

    private Promotion() { }

    private Promotion(
        Guid id,
        string tenantId,
        string name,
        string? description,
        PromotionType promotionType,
        decimal discountValue,
        string currency,
        int? minQuantity,
        int? maxQuantity,
        DateTime validFrom,
        DateTime? validTo,
        bool isStackable,
        int priority,
        string? code,
        int? maxRedemptions)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        PromotionType = promotionType;
        DiscountValue = discountValue;
        Currency = currency;
        MinQuantity = minQuantity;
        MaxQuantity = maxQuantity;
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsActive = true;
        IsStackable = isStackable;
        Priority = priority;
        Code = code;
        MaxRedemptions = maxRedemptions;
        CurrentRedemptions = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public PromotionType PromotionType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public int? MinQuantity { get; private set; }
    public int? MaxQuantity { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsStackable { get; private set; }
    public int Priority { get; private set; }
    public string? Code { get; private set; }
    public int? MaxRedemptions { get; private set; }
    public int CurrentRedemptions { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<PromotionRule> Rules => _rules.AsReadOnly();

    public static Promotion Create(
        string tenantId,
        string name,
        string? description,
        PromotionType promotionType,
        decimal discountValue,
        string currency,
        int? minQuantity,
        int? maxQuantity,
        DateTime validFrom,
        DateTime? validTo,
        bool isStackable,
        int priority,
        string? code,
        int? maxRedemptions)
    {
        return new Promotion(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            promotionType,
            discountValue,
            currency,
            minQuantity,
            maxQuantity,
            validFrom,
            validTo,
            isStackable,
            priority,
            code,
            maxRedemptions);
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

    public bool IsValid()
    {
        if (!IsActive)
            return false;

        if (DateTime.UtcNow < ValidFrom)
            return false;

        if (ValidTo.HasValue && DateTime.UtcNow > ValidTo.Value)
            return false;

        if (MaxRedemptions.HasValue && CurrentRedemptions >= MaxRedemptions.Value)
            return false;

        return true;
    }

    public bool IsApplicable(decimal amount, int quantity)
    {
        if (!IsValid())
            return false;

        if (MinQuantity.HasValue && quantity < MinQuantity.Value)
            return false;

        if (MaxQuantity.HasValue && quantity > MaxQuantity.Value)
            return false;

        if ((PromotionType == PromotionType.Percentage || PromotionType == PromotionType.FixedAmount) && amount <= 0)
            return false;

        return true;
    }

    public decimal CalculateDiscount(decimal amount)
    {
        if (amount <= 0)
            return 0;

        return PromotionType switch
        {
            PromotionType.Percentage => amount * DiscountValue / 100m,
            PromotionType.FixedAmount => Math.Min(DiscountValue, amount),
            PromotionType.FreePeriod => DiscountValue,
            PromotionType.Bundle => DiscountValue,
            PromotionType.Volume => amount * DiscountValue / 100m,
            _ => 0
        };
    }

    public void IncrementRedemptions()
    {
        CurrentRedemptions++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyPromotion(decimal discount, string currency, Guid? subscriptionId, Guid? recordId)
    {
        CurrentRedemptions++;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PromotionAppliedDomainEvent(
            Id, Name, discount, currency, subscriptionId, recordId));
    }

    public void AddRule(PromotionRule rule)
    {
        _rules.Add(rule);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRule(PromotionRule rule)
    {
        _rules.Remove(rule);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearRules()
    {
        _rules.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
