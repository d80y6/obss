using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Application.DTOs;

public sealed record ProductConfigurationRuleDto(
    Guid Id,
    Guid ProductId,
    RuleType RuleType,
    Guid? TargetProductId,
    string? TargetOption,
    string? Condition,
    bool IsActive);

public sealed record ConfigurationValidationResultDto(
    bool IsValid,
    List<ConfigurationValidationErrorDto> Errors);

public sealed record ConfigurationValidationErrorDto(
    string? OptionName,
    string? Value,
    string Rule,
    string Message);

public sealed record SelectedOptionDto(
    Guid OptionId,
    List<string> SelectedValues);
