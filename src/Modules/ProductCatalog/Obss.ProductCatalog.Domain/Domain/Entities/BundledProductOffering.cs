using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class BundledProductOffering : Entity<Guid>
{
    private BundledProductOffering() { }

    public BundledProductOffering(
        Guid id,
        Guid offerId,
        Guid bundledOfferId,
        string? name,
        int quantity,
        string? referralType)
        : base(id)
    {
        OfferId = offerId;
        BundledOfferId = bundledOfferId;
        Name = name;
        Quantity = quantity;
        ReferralType = referralType;
    }

    public Guid OfferId { get; private set; }
    public Guid BundledOfferId { get; private set; }
    public Offer? BundledOffer { get; }
    public string? Name { get; private set; }
    public int Quantity { get; private set; }
    public string? ReferralType { get; private set; }

    public void Update(string? name, int quantity, string? referralType)
    {
        Name = name;
        Quantity = quantity;
        ReferralType = referralType;
    }
}
