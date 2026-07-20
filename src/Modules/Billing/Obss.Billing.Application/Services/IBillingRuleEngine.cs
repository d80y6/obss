using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Services;

public sealed record BillingRuleResult(
    IReadOnlyCollection<BillLine> Lines,
    decimal TotalDiscount,
    decimal TotalTax,
    decimal GrandTotal);

public sealed record BillingRuleTaxResult(
    IReadOnlyCollection<BillLine> TaxLines,
    decimal TotalTax);

public sealed record CompositeBundle(
    string Code,
    string Name,
    decimal TotalPrice,
    decimal? DiscountPercentage);

public sealed record CreditLimitResult(
    bool IsWithinLimit,
    decimal CurrentBalance,
    decimal CreditLimit,
    decimal RemainingCredit);

public interface IBillingRuleEngine
{
    Task<BillingRuleResult> ApplyCompositeBillingAsync(
        IReadOnlyCollection<BillLine> lines,
        CompositeBundle bundle,
        CancellationToken cancellationToken = default);

    Task<BillingRuleResult> ApplyDiscountsAsync(
        IReadOnlyCollection<BillLine> lines,
        IReadOnlyCollection<DiscountRule> discounts,
        CancellationToken cancellationToken = default);

    Task<BillingRuleTaxResult> CalculateTaxAsync(
        Bill bill,
        decimal taxRate,
        CancellationToken cancellationToken = default);

    Result<decimal> ApplyRounding(decimal amount, int decimalPlaces = 2);

    Task<bool> CheckCreditLimitAsync(
        Guid customerId,
        decimal billAmount,
        CancellationToken cancellationToken = default);
}
