using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Infrastructure.Services;

public sealed class ProductConfigurationValidator : IProductConfigurationValidator
{
    public ConfigurationValidationResultDto Validate(
        List<SelectedOptionDto> selectedOptions,
        List<ProductConfigurationRule> rules,
        List<ProductOption> options)
    {
        var errors = new List<ConfigurationValidationErrorDto>();
        var selectedMap = selectedOptions.ToDictionary(
            so => so.OptionId,
            so => so.SelectedValues ?? []);

        foreach (var rule in rules.Where(r => r.IsActive))
        {
            switch (rule.RuleType)
            {
                case RuleType.Required:
                    ValidateRequiredRule(rule, selectedMap, options, errors);
                    break;
                case RuleType.Allowed:
                    ValidateAllowedRule(rule, selectedMap, options, errors);
                    break;
                case RuleType.Excluded:
                    ValidateExcludedRule(rule, selectedMap, options, errors);
                    break;
                case RuleType.Dependent:
                    ValidateDependentRule(rule, selectedMap, options, errors);
                    break;
            }
        }

        return new ConfigurationValidationResultDto(errors.Count == 0, errors);
    }

    private static void ValidateRequiredRule(
        ProductConfigurationRule rule,
        Dictionary<Guid, List<string>> selectedMap,
        List<ProductOption> options,
        List<ConfigurationValidationErrorDto> errors)
    {
        var option = FindOption(rule.TargetOption, options);
        if (option is null) return;

        if (!selectedMap.TryGetValue(option.Id, out var values) || values.Count == 0)
        {
            errors.Add(new ConfigurationValidationErrorDto(
                option.Name,
                null,
                "Required",
                $"Option '{option.Name}' is required."));
        }
    }

    private static void ValidateAllowedRule(
        ProductConfigurationRule rule,
        Dictionary<Guid, List<string>> selectedMap,
        List<ProductOption> options,
        List<ConfigurationValidationErrorDto> errors)
    {
        var option = FindOption(rule.TargetOption, options);
        if (option is null) return;

        if (!selectedMap.TryGetValue(option.Id, out var values) || values.Count == 0)
            return;

        if (string.IsNullOrWhiteSpace(rule.Condition))
            return;

        var allowedValues = rule.Condition
            .Trim('[', ']', '"')
            .Split(',')
            .Select(v => v.Trim().Trim('"'))
            .ToHashSet();

        foreach (var value in values)
        {
            if (!allowedValues.Contains(value))
            {
                errors.Add(new ConfigurationValidationErrorDto(
                    option.Name,
                    value,
                    "Allowed",
                    $"Value '{value}' is not allowed for option '{option.Name}'."));
            }
        }
    }

    private static void ValidateExcludedRule(
        ProductConfigurationRule rule,
        Dictionary<Guid, List<string>> selectedMap,
        List<ProductOption> options,
        List<ConfigurationValidationErrorDto> errors)
    {
        if (rule.TargetProductId is null)
            return;

        var targetProductOption = FindOption(rule.TargetOption, options);
        if (targetProductOption is null) return;

        if (!selectedMap.TryGetValue(targetProductOption.Id, out var targetValues) || targetValues.Count == 0)
            return;

        var condition = rule.Condition;
        if (string.IsNullOrWhiteSpace(condition))
            return;

        var excludedValues = condition
            .Trim('[', ']', '"')
            .Split(',')
            .Select(v => v.Trim().Trim('"'))
            .ToHashSet();

        foreach (var value in targetValues.Where(v => excludedValues.Contains(v)))
        {
            errors.Add(new ConfigurationValidationErrorDto(
                targetProductOption.Name,
                value,
                "Excluded",
                $"Value '{value}' is excluded for option '{targetProductOption.Name}'."));
        }
    }

    private static void ValidateDependentRule(
        ProductConfigurationRule rule,
        Dictionary<Guid, List<string>> selectedMap,
        List<ProductOption> options,
        List<ConfigurationValidationErrorDto> errors)
    {
        if (rule.TargetProductId is null)
            return;

        var targetOption = FindOption(rule.TargetOption, options);
        if (targetOption is null) return;

        var sourceOption = options.FirstOrDefault(o => o.ProductId == rule.TargetProductId.Value);
        if (sourceOption is null) return;

        bool sourceSelected = selectedMap.TryGetValue(sourceOption.Id, out var sourceValues) && sourceValues.Count > 0;

        bool targetSelected = selectedMap.TryGetValue(targetOption.Id, out var targetVals) && targetVals.Count > 0;

        if (sourceSelected && !targetSelected && string.IsNullOrWhiteSpace(rule.Condition))
        {
            errors.Add(new ConfigurationValidationErrorDto(
                targetOption.Name,
                null,
                "Dependent",
                $"Option '{targetOption.Name}' is required when '{sourceOption.Name}' is selected."));
        }

        if (sourceSelected && !string.IsNullOrWhiteSpace(rule.Condition))
        {
            var requiredSourceValues = rule.Condition
                .Trim('[', ']', '"')
                .Split(',')
                .Select(v => v.Trim().Trim('"'))
                .ToHashSet();

            if (sourceValues!.Any(sv => requiredSourceValues.Contains(sv)) && !targetSelected)
            {
                errors.Add(new ConfigurationValidationErrorDto(
                    targetOption.Name,
                    null,
                    "Dependent",
                    $"Option '{targetOption.Name}' is required when '{sourceOption.Name}' has one of the specified values."));
            }
        }
    }

    private static ProductOption? FindOption(string? targetOption, List<ProductOption> options)
    {
        if (string.IsNullOrWhiteSpace(targetOption))
            return null;

        return options.FirstOrDefault(o =>
            o.Name.Equals(targetOption, StringComparison.OrdinalIgnoreCase));
    }
}
