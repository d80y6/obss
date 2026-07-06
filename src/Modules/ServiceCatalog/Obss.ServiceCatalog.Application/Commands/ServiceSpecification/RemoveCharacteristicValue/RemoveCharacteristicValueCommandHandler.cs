using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.RemoveCharacteristicValue;

internal sealed class RemoveCharacteristicValueCommandHandler(IServiceSpecificationRepository repository) : IRequestHandler<RemoveCharacteristicValueCommand>
{
    public async Task Handle(RemoveCharacteristicValueCommand request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdWithCharacteristicsAsync(request.ServiceSpecificationId, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service specification {request.ServiceSpecificationId} not found");

        var characteristic = spec.Characteristics.FirstOrDefault(c => c.Id == request.CharacteristicId)
            ?? throw new ServiceCatalogDomainException($"Characteristic {request.CharacteristicId} not found");

        characteristic.RemoveValue(request.ValueId);
        await repository.UpdateAsync(spec, cancellationToken);
    }
}
