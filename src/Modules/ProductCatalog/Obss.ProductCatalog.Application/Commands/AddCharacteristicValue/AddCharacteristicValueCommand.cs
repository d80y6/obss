using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.AddCharacteristicValue;

public sealed record AddCharacteristicValueCommand(
    Guid ProductSpecificationId,
    Guid CharacteristicId,
    string Value,
    string? UnitOfMeasure,
    bool IsDefault,
    string? ValueFrom,
    string? ValueTo,
    string? RangeInterval,
    DateTime? ValidFrom,
    DateTime? ValidTo) : IRequest<Result<ProductSpecificationCharacteristicValueDto>>;
