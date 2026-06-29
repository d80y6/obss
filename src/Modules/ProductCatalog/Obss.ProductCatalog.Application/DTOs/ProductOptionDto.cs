using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Application.DTOs;

public sealed record ProductOptionDto(
    Guid Id,
    Guid ProductId,
    string Name,
    string? Description,
    OptionType OptionType,
    bool IsRequired,
    bool IsMultiSelect,
    int SortOrder,
    bool IsActive,
    List<OptionValueDto> Values);

public sealed record OptionValueDto(
    Guid Id,
    Guid ProductOptionId,
    string Value,
    string DisplayName,
    decimal PriceAdjustment,
    bool IsDefault,
    bool IsActive);

public sealed record CreateOptionValueDto(
    string Value,
    string DisplayName,
    decimal PriceAdjustment,
    bool IsDefault);

public sealed record ProductConfigurationDto(
    Guid ProductId,
    List<ProductConfigurationRuleDto> Rules,
    List<ProductOptionDto> Options);
