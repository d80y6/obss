using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.UpdateCharacteristicValue;

internal sealed class UpdateCharacteristicValueCommandHandler(IServiceSpecificationRepository repository) : IRequestHandler<UpdateCharacteristicValueCommand>
{
    public async Task Handle(UpdateCharacteristicValueCommand request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdWithCharacteristicsAsync(request.ServiceSpecificationId, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service specification {request.ServiceSpecificationId} not found");

        var characteristic = spec.Characteristics.FirstOrDefault(c => c.Id == request.CharacteristicId)
            ?? throw new ServiceCatalogDomainException($"Characteristic {request.CharacteristicId} not found");

        var value = characteristic.Values.FirstOrDefault(v => v.Id == request.ValueId)
            ?? throw new ServiceCatalogDomainException($"Value {request.ValueId} not found");

        value.Update(
            request.Value, request.UnitOfMeasure, request.IsDefault,
            request.ValueFrom, request.ValueTo, request.RangeInterval,
            request.ValidFrom, request.ValidTo);

        await repository.UpdateAsync(spec, cancellationToken);
    }
}
