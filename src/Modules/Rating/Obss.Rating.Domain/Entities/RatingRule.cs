using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Rating.Domain.Entities;

public class RatingRule : AggregateRoot<Guid>
{
    private readonly List<RatingTier> _tiers = [];

    private RatingRule() { }

    private RatingRule(
        Guid id,
        string tenantId,
        string name,
        string? description,
        RatingRuleType ruleType,
        Guid? productId,
        Guid? offerId,
        int priority)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        RuleType = ruleType;
        ProductId = productId;
        OfferId = offerId;
        IsActive = true;
        Priority = priority;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public RatingRuleType RuleType { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? OfferId { get; private set; }
    public bool IsActive { get; private set; }
    public int Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? ExternalId { get; private set; }
#pragma warning restore S1144

    public IReadOnlyCollection<RatingTier> Tiers => _tiers.AsReadOnly();

    public static RatingRule Create(
        string tenantId,
        string name,
        string? description,
        RatingRuleType ruleType,
        Guid? productId,
        Guid? offerId,
        int priority)
    {
        return new RatingRule(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            ruleType,
            productId,
            offerId,
            priority);
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

    public void SetPriority(int priority)
    {
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTier(RatingTier tier)
    {
        _tiers.Add(tier);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveTier(RatingTier tier)
    {
        _tiers.Remove(tier);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearTiers()
    {
        _tiers.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}

public sealed class RatingTier
{
    private RatingTier() { }

    public RatingTier(int fromUnit, int? toUnit, decimal rate, string? description)
    {
        FromUnit = fromUnit;
        ToUnit = toUnit;
        Rate = rate;
        Description = description;
    }

    public int Id { get; set; }
    public int FromUnit { get; private set; }
    public int? ToUnit { get; private set; }
    public decimal Rate { get; private set; }
    public string? Description { get; private set; }

    public decimal CalculateCost(long units)
    {
        if (ToUnit.HasValue && units > ToUnit.Value)
            units = ToUnit.Value - FromUnit + 1;

        if (units <= 0)
            return 0;

        return units * Rate;
    }

    public bool IsInTier(long units)
    {
        return units >= FromUnit && (!ToUnit.HasValue || units <= ToUnit.Value);
    }
}
