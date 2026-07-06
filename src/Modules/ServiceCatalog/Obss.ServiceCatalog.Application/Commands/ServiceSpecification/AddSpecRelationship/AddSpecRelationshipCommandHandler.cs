using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Entities;
using Obss.ServiceCatalog.Domain.Enums;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.AddSpecRelationship;

internal sealed class AddSpecRelationshipCommandHandler(IServiceSpecificationRepository repository) : IRequestHandler<AddSpecRelationshipCommand, Guid>
{
    public async Task<Guid> Handle(AddSpecRelationshipCommand request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdAsync(request.ServiceSpecificationId, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service specification {request.ServiceSpecificationId} not found");

        var relationshipType = Enum.Parse<RelationshipType>(request.RelationshipType);
        var relationship = new ServiceSpecRelationship(
            Guid.NewGuid(),
            request.ServiceSpecificationId,
            request.TargetSpecificationId,
            relationshipType,
            request.Role,
            request.ValidFrom,
            request.ValidTo);

        spec.AddRelationship(relationship);
        await repository.UpdateAsync(spec, cancellationToken);
        return relationship.Id;
    }
}
