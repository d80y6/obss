using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.UpdateCharacteristicValue;

public sealed record UpdateCharacteristicValueCommand(
    Guid ServiceSpecificationId,
    Guid CharacteristicId,
    Guid ValueId,
    string Value,
    string? UnitOfMeasure,
    bool IsDefault,
    string? ValueFrom,
    string? ValueTo,
    string? RangeInterval,
    DateTime? ValidFrom,
    DateTime? ValidTo
) : IRequest;
