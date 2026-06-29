using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Events;

public sealed class OfferCreatedDomainEvent : DomainEvent
{
    public OfferCreatedDomainEvent(Guid offerId, string tenantId, string name, OfferType offerType)
    {
        OfferId = offerId;
        TenantId = tenantId;
        Name = name;
        OfferType = offerType;
    }

    public Guid OfferId { get; }
    public string TenantId { get; }
    public string Name { get; }
    public OfferType OfferType { get; }
}
