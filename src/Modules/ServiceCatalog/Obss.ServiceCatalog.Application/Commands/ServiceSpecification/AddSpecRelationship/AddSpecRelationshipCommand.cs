using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.AddSpecRelationship;

public sealed record AddSpecRelationshipCommand(
    Guid ServiceSpecificationId,
    Guid TargetSpecificationId,
    string RelationshipType,
    string? Role,
    DateTime? ValidFrom,
    DateTime? ValidTo
) : IRequest<Guid>;
