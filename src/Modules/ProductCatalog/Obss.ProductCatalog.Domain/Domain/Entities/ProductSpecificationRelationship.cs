using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ProductCatalog.Domain.Domain.Entities;

public class ProductSpecificationRelationship : Entity<Guid>
{
    private ProductSpecificationRelationship() { }

    public ProductSpecificationRelationship(
        Guid id,
        Guid productSpecificationId,
        Guid targetSpecificationId,
        SpecificationRelationshipType relationshipType,
        string? role,
        DateTime? validFrom,
        DateTime? validTo)
        : base(id)
    {
        ProductSpecificationId = productSpecificationId;
        TargetSpecificationId = targetSpecificationId;
        RelationshipType = relationshipType;
        Role = role;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public Guid ProductSpecificationId { get; private set; }
    public Guid TargetSpecificationId { get; private set; }
    public SpecificationRelationshipType RelationshipType { get; private set; }
    public string? Role { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
}
