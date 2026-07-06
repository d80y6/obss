namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceSpecCharValueDto(
    Guid Id,
    Guid CharacteristicId,
    string Value,
    string? UnitOfMeasure,
    bool IsDefault,
    string? ValueFrom,
    string? ValueTo,
    string? RangeInterval,
    DateTime? ValidFrom,
    DateTime? ValidTo
);
