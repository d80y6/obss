using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateCharacteristicValue;

public sealed record UpdateCharacteristicValueCommand(
    Guid ProductSpecificationId,
    Guid CharacteristicId,
    Guid ValueId,
    string Value,
    string? UnitOfMeasure,
    bool IsDefault,
    string? ValueFrom,
    string? ValueTo,
    string? RangeInterval,
    DateTime? ValidFrom,
    DateTime? ValidTo) : IRequest<Result<ProductSpecificationCharacteristicValueDto>>;
