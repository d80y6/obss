using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateCharacteristic;

public sealed record UpdateCharacteristicCommand(
    Guid ProductSpecificationId,
    Guid CharacteristicId,
    string Name,
    string? Description,
    string ValueType,
    bool Configurable,
    int? MinValue,
    int? MaxValue,
    string? Regex,
    int SortOrder,
    int? MaxCardinality,
    bool IsRequired) : IRequest<Result<ProductSpecificationCharacteristicDto>>;
