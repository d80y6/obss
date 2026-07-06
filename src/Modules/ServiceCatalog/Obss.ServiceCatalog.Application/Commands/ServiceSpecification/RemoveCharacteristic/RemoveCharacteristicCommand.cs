using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.RemoveCharacteristic;

public sealed record RemoveCharacteristicCommand(
    Guid ServiceSpecificationId,
    Guid CharacteristicId
) : IRequest;
