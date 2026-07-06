using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Entities;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.AddCharacteristic;

internal sealed class AddCharacteristicCommandHandler(IServiceSpecificationRepository repository) : IRequestHandler<AddCharacteristicCommand, Guid>
{
    public async Task<Guid> Handle(AddCharacteristicCommand request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdWithCharacteristicsAsync(request.ServiceSpecificationId, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service specification {request.ServiceSpecificationId} not found");

        var characteristic = new ServiceSpecCharacteristic(
            Guid.NewGuid(),
            request.ServiceSpecificationId,
            request.Name,
            request.Description,
            request.ValueType,
            request.Configurable,
            request.MinValue,
            request.MaxValue,
            request.Regex,
            request.SortOrder,
            request.MaxCardinality,
            request.IsRequired);

        spec.AddCharacteristic(characteristic);
        await repository.UpdateAsync(spec, cancellationToken);
        return characteristic.Id;
    }
}
