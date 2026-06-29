using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class OfferDiscount : Entity<Guid>
{
    private OfferDiscount() { }

    public OfferDiscount(
        Guid id,
        Guid offerId,
        DiscountType discountType,
        decimal discountValue,
        int? discountPeriodMonths,
        DateTime? validFrom,
        DateTime? validTo,
        bool isActive,
        string? description)
        : base(id)
    {
        OfferId = offerId;
        DiscountType = discountType;
        DiscountValue = discountValue;
        DiscountPeriodMonths = discountPeriodMonths;
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsActive = isActive;
        Description = description;
    }

    public Guid OfferId { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public int? DiscountPeriodMonths { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public bool IsActive { get; private set; }
    public string? Description { get; private set; }
}
