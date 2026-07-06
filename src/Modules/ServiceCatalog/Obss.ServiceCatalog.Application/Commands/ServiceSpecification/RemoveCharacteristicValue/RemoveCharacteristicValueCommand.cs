using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.RemoveCharacteristicValue;

public sealed record RemoveCharacteristicValueCommand(
    Guid ServiceSpecificationId,
    Guid CharacteristicId,
    Guid ValueId
) : IRequest;
