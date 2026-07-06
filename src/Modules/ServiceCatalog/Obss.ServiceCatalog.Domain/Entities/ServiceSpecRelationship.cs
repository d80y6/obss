using Obss.ServiceCatalog.Domain.Enums;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceCatalog.Domain.Entities;

public class ServiceSpecRelationship : Entity<Guid>
{
    public Guid ServiceSpecificationId { get; private set; }
    public Guid TargetSpecificationId { get; private set; }
    public RelationshipType RelationshipType { get; private set; }
    public string? Role { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    private ServiceSpecRelationship() { }

    public ServiceSpecRelationship(Guid id, Guid serviceSpecificationId, Guid targetSpecificationId, RelationshipType relationshipType, string? role, DateTime? validFrom, DateTime? validTo) : base(id)
    {
        ServiceSpecificationId = serviceSpecificationId;
        TargetSpecificationId = targetSpecificationId;
        RelationshipType = relationshipType;
        Role = role;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }
}
