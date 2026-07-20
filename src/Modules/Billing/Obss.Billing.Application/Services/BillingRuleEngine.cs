using Microsoft.Extensions.Logging;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Services;

public sealed class BillingRuleEngine : IBillingRuleEngine
{
    private readonly ILogger<BillingRuleEngine> _logger;

    public BillingRuleEngine(ILogger<BillingRuleEngine> logger)
    {
        _logger = logger;
    }

    public Task<BillingRuleResult> ApplyCompositeBillingAsync(
        IReadOnlyCollection<BillLine> lines,
        CompositeBundle bundle,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var total = lines.Sum(l => l.UnitPrice * l.Quantity);
        var bundleDiscount = bundle.DiscountPercentage.HasValue
            ? total * (bundle.DiscountPercentage.Value / 100m)
            : 0m;

        var discountedLines = lines.Select(line =>
        {
            var discount = line.UnitPrice * line.Quantity * (bundle.DiscountPercentage ?? 0) / 100m;
            var lineDiscount = Math.Min(discount, line.UnitPrice * line.Quantity - 0.01m);
            return BillLine.CreateAdjustment(
                line.BillId,
                $"Bundle discount ({bundle.Name}): {line.Description}",
                -lineDiscount,
                line.Currency,
                line.LineDate);
        }).ToList();

        _logger.LogInformation(
            "Applied composite bundle {BundleCode}: {Total} with {Discount} discount",
            bundle.Code, total, bundleDiscount);

        var result = new BillingRuleResult(discountedLines.AsReadOnly(), bundleDiscount, 0, total - bundleDiscount);
        return Task.FromResult(result);
    }

    public Task<BillingRuleResult> ApplyDiscountsAsync(
        IReadOnlyCollection<BillLine> lines,
        IReadOnlyCollection<DiscountRule> discounts,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var subtotal = lines.Sum(l => l.UnitPrice * l.Quantity);
        var totalDiscount = 0m;

        foreach (var discount in discounts)
        {
            if (!discount.IsStackable && totalDiscount > 0)
            {
                _logger.LogInformation("Skipping non-stackable discount {Name} (already applied)", discount.Name);
                continue;
            }

            var amount = subtotal * (discount.Percentage / 100m);
            if (discount.MaxDiscount.HasValue && amount > discount.MaxDiscount.Value)
            {
                amount = discount.MaxDiscount.Value;
            }

            totalDiscount += amount;
        }

        var grandTotal = subtotal - totalDiscount;
        _logger.LogInformation(
            "Applied {Count} discounts: {TotalDiscount} discount, grand total {GrandTotal}",
            discounts.Count, totalDiscount, grandTotal);

        return Task.FromResult(new BillingRuleResult(lines, totalDiscount, 0, grandTotal));
    }

    public Task<BillingRuleTaxResult> CalculateTaxAsync(
        Bill bill,
        decimal taxRate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var taxableLines = bill.Lines.Where(l => l.LineType != LineType.Tax).ToList();
        var taxLines = new List<BillLine>();

        foreach (var line in taxableLines)
        {
            var lineTotal = line.UnitPrice * line.Quantity - line.DiscountAmount;
            var taxAmount = lineTotal * taxRate;

            if (taxAmount <= 0)
            {
                continue;
            }

            var taxLine = BillLine.CreateTaxLine(
                bill.Id,
                $"Tax ({taxRate * 100}%) - {line.Description}",
                taxAmount,
                taxRate,
                line.Currency,
                line.LineDate);

            taxLines.Add(taxLine);
        }

        var totalTax = taxLines.Sum(t => t.UnitPrice);
        var roundedTax = ApplyRounding(totalTax);

        _logger.LogInformation("Calculated tax: {TotalTax} at rate {Rate}", roundedTax.Value, taxRate);

        return Task.FromResult(new BillingRuleTaxResult(taxLines.AsReadOnly(), roundedTax.Value));
    }

    public Result<decimal> ApplyRounding(decimal amount, int decimalPlaces = 2)
    {
        var rounded = Math.Round(amount, decimalPlaces, MidpointRounding.AwayFromZero);
        return Result.Success(rounded);
    }

    public Task<bool> CheckCreditLimitAsync(
        Guid customerId,
        decimal billAmount,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Credit limit check for customer {CustomerId}: bill amount {BillAmount}",
            customerId, billAmount);

        return Task.FromResult(true);
    }
}
