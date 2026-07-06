using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.UpdateCharacteristic;

public sealed record UpdateCharacteristicCommand(
    Guid ServiceSpecificationId,
    Guid CharacteristicId,
    string Name,
    string? Description,
    string ValueType,
    bool Configurable,
    decimal? MinValue,
    decimal? MaxValue,
    string? Regex,
    int SortOrder,
    int? MaxCardinality,
    bool IsRequired
) : IRequest;
