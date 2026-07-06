using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Entities;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.AddCharacteristicValue;

internal sealed class AddCharacteristicValueCommandHandler(IServiceSpecificationRepository repository) : IRequestHandler<AddCharacteristicValueCommand, Guid>
{
    public async Task<Guid> Handle(AddCharacteristicValueCommand request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdWithCharacteristicsAsync(request.ServiceSpecificationId, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service specification {request.ServiceSpecificationId} not found");

        var characteristic = spec.Characteristics.FirstOrDefault(c => c.Id == request.CharacteristicId)
            ?? throw new ServiceCatalogDomainException($"Characteristic {request.CharacteristicId} not found");

        var value = new ServiceSpecCharValue(
            Guid.NewGuid(),
            request.CharacteristicId,
            request.Value,
            request.UnitOfMeasure,
            request.IsDefault,
            request.ValueFrom,
            request.ValueTo,
            request.RangeInterval,
            request.ValidFrom,
            request.ValidTo);

        characteristic.AddValue(value);
        await repository.UpdateAsync(spec, cancellationToken);
        return value.Id;
    }
}
