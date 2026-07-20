namespace Obss.Billing.Application.Services;

public sealed record DiscountRule(
    string Name,
    string Code,
    decimal Percentage,
    decimal? MaxDiscount,
    bool IsStackable);
