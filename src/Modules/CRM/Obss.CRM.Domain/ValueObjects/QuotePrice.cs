namespace Obss.CRM.Domain.ValueObjects;

public sealed record QuotePrice(
    PriceType PriceType,
    string Name,
    decimal DutyFreeAmount,
    decimal TaxIncludedAmount,
    decimal TaxRate,
    string Currency,
    string? UnitOfMeasure,
    int? RecurringPeriod,
    string? RecurringPeriodUnit);
