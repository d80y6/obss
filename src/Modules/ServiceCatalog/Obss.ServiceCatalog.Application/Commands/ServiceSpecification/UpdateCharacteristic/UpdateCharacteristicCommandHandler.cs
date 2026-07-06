using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.UpdateCharacteristic;

internal sealed class UpdateCharacteristicCommandHandler(IServiceSpecificationRepository repository) : IRequestHandler<UpdateCharacteristicCommand>
{
    public async Task Handle(UpdateCharacteristicCommand request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdWithCharacteristicsAsync(request.ServiceSpecificationId, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service specification {request.ServiceSpecificationId} not found");

        var characteristic = spec.Characteristics.FirstOrDefault(c => c.Id == request.CharacteristicId)
            ?? throw new ServiceCatalogDomainException($"Characteristic {request.CharacteristicId} not found");

        characteristic.UpdateDetails(
            request.Name, request.Description, request.ValueType, request.Configurable,
            request.MinValue, request.MaxValue, request.Regex, request.SortOrder,
            request.MaxCardinality, request.IsRequired);

        await repository.UpdateAsync(spec, cancellationToken);
    }
}
