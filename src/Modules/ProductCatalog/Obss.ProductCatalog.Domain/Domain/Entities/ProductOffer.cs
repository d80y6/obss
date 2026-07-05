using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class ProductOffer : Entity<Guid>
{
    private ProductOffer() { }

    public ProductOffer(Guid id, Guid productId, Guid offerId, bool isPrimary, bool isRequired)
        : base(id)
    {
        ProductId = productId;
        OfferId = offerId;
        IsPrimary = isPrimary;
        IsRequired = isRequired;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ProductId { get; private set; }
    public Product? Product { get; }
    public Guid OfferId { get; private set; }
    public Offer? Offer { get; }
    public bool IsPrimary { get; private set; }
    public bool IsRequired { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
