namespace Obss.CRM.Domain.ValueObjects;

public sealed record PriceAlteration(
    string Name,
    string? Description,
    PriceType PriceType,
    int? ApplicationDuration,
    int Priority,
    decimal DutyFreeAmount,
    decimal TaxIncludedAmount,
    decimal TaxRate,
    string Currency);
