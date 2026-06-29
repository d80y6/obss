using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Events;

public sealed class OfferPricingChangedDomainEvent : DomainEvent
{
    public OfferPricingChangedDomainEvent(Guid offerId, Guid offerPricingId, PricingType pricingType)
    {
        OfferId = offerId;
        OfferPricingId = offerPricingId;
        PricingType = pricingType;
    }

    public Guid OfferId { get; }
    public Guid OfferPricingId { get; }
    public PricingType PricingType { get; }
}
