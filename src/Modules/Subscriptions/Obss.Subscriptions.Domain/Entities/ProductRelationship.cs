using Obss.SharedKernel.Domain.Common;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Domain.Entities;

public class ProductRelationship : Entity<Guid>
{
    private ProductRelationship() { }

    public ProductRelationship(Guid id, Guid relatedProductId, ProductRelationshipType type)
        : base(id)
    {
        RelatedProductId = relatedProductId;
        Type = type;
    }

    public Guid RelatedProductId { get; private set; }
    public ProductRelationshipType Type { get; private set; }
}
