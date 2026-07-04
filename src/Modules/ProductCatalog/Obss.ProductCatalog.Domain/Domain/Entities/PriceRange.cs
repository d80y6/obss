using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class PriceRange : Entity<Guid>
{
    private PriceRange() { }

    public PriceRange(Guid id, Guid offerPricingId, int minQuantity, int? maxQuantity, decimal price, bool isActive)
        : base(id)
    {
        OfferPricingId = offerPricingId;
        MinQuantity = minQuantity;
        MaxQuantity = maxQuantity;
        Price = price;
        IsActive = isActive;
    }

    public Guid OfferPricingId { get; private set; }
    public int MinQuantity { get; private set; }
    public int? MaxQuantity { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(int minQuantity, int? maxQuantity, decimal price, bool isActive)
    {
        MinQuantity = minQuantity;
        MaxQuantity = maxQuantity;
        Price = price;
        IsActive = isActive;
    }
}
