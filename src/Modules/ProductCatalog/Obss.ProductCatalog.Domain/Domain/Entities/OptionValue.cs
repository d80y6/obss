using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class OptionValue : Entity<Guid>
{
    private OptionValue() { }

    private OptionValue(
        Guid id,
        Guid productOptionId,
        string value,
        string displayName,
        decimal priceAdjustment,
        bool isDefault)
        : base(id)
    {
        ProductOptionId = productOptionId;
        Value = value;
        DisplayName = displayName;
        PriceAdjustment = priceAdjustment;
        IsDefault = isDefault;
        IsActive = true;
    }

    public Guid ProductOptionId { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public decimal PriceAdjustment { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    public static OptionValue Create(
        Guid productOptionId,
        string value,
        string displayName,
        decimal priceAdjustment,
        bool isDefault)
    {
        return new OptionValue(
            Guid.NewGuid(),
            productOptionId,
            value,
            displayName,
            priceAdjustment,
            isDefault);
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }
}
