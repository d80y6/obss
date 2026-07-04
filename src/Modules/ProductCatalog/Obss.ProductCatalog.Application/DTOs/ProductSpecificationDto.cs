using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Application.DTOs;

public sealed record ProductSpecificationDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string? Brand,
    string? Version,
    string? ProductNumber,
    LifecycleStatus LifecycleStatus,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ProductSpecificationCharacteristicDto> Characteristics,
    List<ProductSpecificationRelationshipDto> Relationships);

public sealed record ProductSpecificationCharacteristicDto(
    Guid Id,
    Guid ProductSpecificationId,
    string Name,
    string? Description,
    string ValueType,
    bool Configurable,
    decimal? MinValue,
    decimal? MaxValue,
    string? Regex,
    int SortOrder,
    int? MaxCardinality,
    bool IsRequired,
    List<ProductSpecificationCharacteristicValueDto> Values);

public sealed record ProductSpecificationCharacteristicValueDto(
    Guid Id,
    Guid CharacteristicId,
    string Value,
    string? UnitOfMeasure,
    bool IsDefault,
    string? ValueFrom,
    string? ValueTo,
    string? RangeInterval,
    DateTime? ValidFrom,
    DateTime? ValidTo);

public sealed record ProductSpecificationRelationshipDto(
    Guid Id,
    Guid ProductSpecificationId,
    Guid TargetSpecificationId,
    SpecificationRelationshipType RelationshipType,
    string? Role,
    DateTime? ValidFrom,
    DateTime? ValidTo);
