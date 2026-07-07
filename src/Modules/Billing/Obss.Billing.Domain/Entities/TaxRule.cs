using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Billing.Domain.Entities;

public class TaxRule : AggregateRoot<Guid>, ITenantEntity
{
    private TaxRule() { }

    private TaxRule(
        Guid id,
        string tenantId,
        string name,
        string description,
        decimal taxRate,
        TaxType taxType,
        string taxCategory,
        string country,
        string region,
        bool isCompound,
        int priority,
        DateTime effectiveFrom,
        DateTime? effectiveTo)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        TaxRate = taxRate;
        TaxType = taxType;
        TaxCategory = taxCategory;
        Country = country;
        Region = region;
        IsCompound = isCompound;
        Priority = priority;
        IsActive = true;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal TaxRate { get; private set; }
    public TaxType TaxType { get; private set; }
    public string TaxCategory { get; private set; } = string.Empty;
    public string Country { get; private set; } = "YE";
    public string Region { get; private set; } = string.Empty;
    public bool IsCompound { get; private set; }
    public bool IsActive { get; private set; }
    public int Priority { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public DateTime CreatedAt { get; private set; }
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? ExternalId { get; private set; }
#pragma warning restore S1144

    public static TaxRule Create(
        string tenantId,
        string name,
        string description,
        decimal taxRate,
        TaxType taxType,
        string taxCategory,
        string country,
        string region,
        bool isCompound,
        int priority,
        DateTime effectiveFrom,
        DateTime? effectiveTo)
    {
        return new TaxRule(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            taxRate,
            taxType,
            taxCategory,
            country,
            region,
            isCompound,
            priority,
            effectiveFrom,
            effectiveTo);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool IsApplicable(string category, string country)
    {
        if (!IsActive)
            return false;

        var now = DateTime.UtcNow;
        if (now < EffectiveFrom)
            return false;

        if (EffectiveTo.HasValue && now > EffectiveTo.Value)
            return false;

        if (!string.IsNullOrWhiteSpace(TaxCategory) &&
            !TaxCategory.Equals(category, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrWhiteSpace(Country) &&
            !Country.Equals(country, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    public decimal CalculateTax(decimal amount)
    {
        return TaxType switch
        {
            TaxType.Percentage => amount * TaxRate,
            TaxType.Fixed => TaxRate,
            _ => 0
        };
    }
}
