using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.AddCharacteristic;

public sealed record AddCharacteristicCommand(
    Guid ProductSpecificationId,
    string Name,
    string? Description,
    string ValueType,
    bool Configurable,
    int? MinValue,
    int? MaxValue,
    string? Regex,
    int SortOrder,
    int? MaxCardinality,
    bool IsRequired,
    List<AddCharacteristicValueItem>? Values) : IRequest<Result<ProductSpecificationCharacteristicDto>>;

public sealed record AddCharacteristicValueItem(
    string Value,
    string? UnitOfMeasure,
    bool IsDefault,
    string? ValueFrom,
    string? ValueTo,
    string? RangeInterval,
    DateTime? ValidFrom,
    DateTime? ValidTo);
