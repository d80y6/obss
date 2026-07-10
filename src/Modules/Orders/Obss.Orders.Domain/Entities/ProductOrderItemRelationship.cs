using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Domain.Entities;

public sealed record ProductOrderItemRelationship
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProductOrderItemId { get; private set; }
    public Guid TargetItemId { get; private set; }
    public RelationshipType Type { get; private set; }

    private ProductOrderItemRelationship() { }

    public ProductOrderItemRelationship(Guid itemId, Guid targetItemId, RelationshipType type)
    {
        Id = Guid.NewGuid();
        ProductOrderItemId = itemId;
        TargetItemId = targetItemId;
        Type = type;
    }
}
