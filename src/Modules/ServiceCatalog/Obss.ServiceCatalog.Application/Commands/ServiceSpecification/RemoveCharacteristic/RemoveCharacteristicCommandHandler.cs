using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.RemoveCharacteristic;

internal sealed class RemoveCharacteristicCommandHandler(IServiceSpecificationRepository repository) : IRequestHandler<RemoveCharacteristicCommand>
{
    public async Task Handle(RemoveCharacteristicCommand request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdWithCharacteristicsAsync(request.ServiceSpecificationId, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service specification {request.ServiceSpecificationId} not found");

        spec.RemoveCharacteristic(request.CharacteristicId);
        await repository.UpdateAsync(spec, cancellationToken);
    }
}
