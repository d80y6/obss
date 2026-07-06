using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.RemoveSpecRelationship;

public sealed record RemoveSpecRelationshipCommand(
    Guid ServiceSpecificationId,
    Guid RelationshipId
) : IRequest;
