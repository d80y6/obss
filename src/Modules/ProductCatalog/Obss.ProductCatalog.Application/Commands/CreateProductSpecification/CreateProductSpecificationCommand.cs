using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateProductSpecification;

public sealed record CreateProductSpecificationCommand(
    string TenantId,
    string Name,
    string? Description,
    string? Brand,
    string? Version,
    string? ProductNumber,
    List<CreateCharacteristicItem>? Characteristics,
    List<CreateRelationshipItem>? Relationships) : IRequest<Result<ProductSpecificationDto>>;

public sealed record CreateCharacteristicItem(
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
    List<CreateCharacteristicValueItem>? Values);

public sealed record CreateCharacteristicValueItem(
    string Value,
    string? UnitOfMeasure,
    bool IsDefault,
    string? ValueFrom,
    string? ValueTo,
    string? RangeInterval,
    DateTime? ValidFrom,
    DateTime? ValidTo);

public sealed record CreateRelationshipItem(
    Guid TargetSpecificationId,
    SpecificationRelationshipType RelationshipType,
    string? Role,
    DateTime? ValidFrom,
    DateTime? ValidTo);
