using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.AddCharacteristicValue;

public sealed record AddCharacteristicValueCommand(
    Guid ServiceSpecificationId,
    Guid CharacteristicId,
    string Value,
    string? UnitOfMeasure,
    bool IsDefault,
    string? ValueFrom,
    string? ValueTo,
    string? RangeInterval,
    DateTime? ValidFrom,
    DateTime? ValidTo
) : IRequest<Guid>;
