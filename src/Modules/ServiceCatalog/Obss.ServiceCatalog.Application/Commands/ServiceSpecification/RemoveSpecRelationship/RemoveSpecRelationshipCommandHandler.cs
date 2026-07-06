using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.RemoveSpecRelationship;

internal sealed class RemoveSpecRelationshipCommandHandler(IServiceSpecificationRepository repository) : IRequestHandler<RemoveSpecRelationshipCommand>
{
    public async Task Handle(RemoveSpecRelationshipCommand request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdAsync(request.ServiceSpecificationId, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service specification {request.ServiceSpecificationId} not found");

        spec.RemoveRelationship(request.RelationshipId);
        await repository.UpdateAsync(spec, cancellationToken);
    }
}
