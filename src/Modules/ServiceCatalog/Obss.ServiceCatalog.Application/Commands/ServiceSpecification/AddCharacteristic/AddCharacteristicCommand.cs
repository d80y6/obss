using MediatR;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.AddCharacteristic;

public sealed record AddCharacteristicCommand(
    Guid ServiceSpecificationId,
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
) : IRequest<Guid>;
