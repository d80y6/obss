namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceSpecCharacteristicDto(
    Guid Id,
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
    bool IsRequired,
    List<ServiceSpecCharValueDto> Values
);
