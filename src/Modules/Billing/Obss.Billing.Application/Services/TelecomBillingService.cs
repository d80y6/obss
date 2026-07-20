using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.Configuration;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Services;

public sealed class TelecomBillingService : ITelecomBillingService
{
    private readonly IBillRepository _billRepository;
    private readonly IBillingRuleEngine _ruleEngine;
    private readonly IOptions<BillingConfiguration> _config;
    private readonly ILogger<TelecomBillingService> _logger;

    public TelecomBillingService(
        IBillRepository billRepository,
        IBillingRuleEngine ruleEngine,
        IOptions<BillingConfiguration> config,
        ILogger<TelecomBillingService> logger)
    {
        _billRepository = billRepository;
        _ruleEngine = ruleEngine;
        _config = config;
        _logger = logger;
    }

    public async Task<Result<Guid>> GenerateBillFromUsageAsync(
        Guid customerId,
        string customerName,
        string tenantId,
        BillingPeriod period,
        DateTime periodStart,
        DateTime periodEnd,
        IReadOnlyCollection<RatedUsageDto> usageRecords,
        RatingPlan plan,
        CancellationToken cancellationToken = default)
    {
        return await GenerateBillWithChargesAsync(
            customerId, customerName, tenantId, period, periodStart, periodEnd,
            usageRecords, [], [], [], plan, cancellationToken);
    }

    public async Task<Result<Guid>> GenerateBillWithChargesAsync(
        Guid customerId,
        string customerName,
        string tenantId,
        BillingPeriod period,
        DateTime periodStart,
        DateTime periodEnd,
        IReadOnlyCollection<RatedUsageDto> usageRecords,
        IReadOnlyCollection<SubscriptionChargeDto> subscriptions,
        IReadOnlyCollection<OneTimeChargeDto> oneTimeCharges,
        IReadOnlyCollection<DiscountRule> discounts,
        RatingPlan plan,
        CancellationToken cancellationToken = default)
    {
        var currency = _config.Value.DefaultCurrency;
        var dueDate = periodEnd.AddDays(15);

        var bill = Bill.Create(
            tenantId,
            customerId,
            customerName,
            period,
            periodStart,
            periodEnd,
            dueDate,
            currency);

        foreach (var usage in usageRecords)
        {
            var line = BillLine.CreateUsage(
                bill.Id,
                usage.Description,
                usage.SubscriptionId,
                null,
                null,
                usage.Quantity,
                usage.UnitPrice,
                usage.DiscountAmount,
                usage.TaxRate,
                currency,
                usage.UsageDate,
                usage.Reference);

            bill.AddLine(line);
        }

        foreach (var sub in subscriptions)
        {
            var proratedFee = ProrateMonthlyFee(sub.MonthlyFee, periodStart, periodEnd, sub.ActivationDate);
            var line = BillLine.CreateRecurring(
                bill.Id,
                $"Subscription - {sub.OfferName}",
                sub.SubscriptionId,
                null,
                null,
                sub.Quantity,
                proratedFee,
                0,
                _config.Value.TaxRate,
                currency,
                periodStart);

            bill.AddLine(line);
        }

        foreach (var charge in oneTimeCharges)
        {
            var line = BillLine.CreateAdjustment(
                bill.Id,
                charge.Description,
                charge.Amount,
                currency,
                periodStart);

            bill.AddLine(line);
        }

        foreach (var discount in discounts)
        {
            var discountAmount = ApplyDiscountToBill(bill, discount);
            if (discountAmount > 0)
            {
                var discountLine = BillLine.CreateAdjustment(
                    bill.Id,
                    $"Discount: {discount.Name}",
                    -discountAmount,
                    currency,
                    periodStart);

                bill.AddLine(discountLine);
            }
        }

        var taxResult = await _ruleEngine.CalculateTaxAsync(bill, _config.Value.TaxRate, cancellationToken);
        if (taxResult.TaxLines.Count > 0)
        {
            foreach (var taxLine in taxResult.TaxLines)
            {
                bill.AddTaxLine(taxLine);
            }
        }

        bill.CalculateTotals();

        var creditOk = await _ruleEngine.CheckCreditLimitAsync(customerId, bill.GrandTotal, cancellationToken);
        if (!creditOk)
        {
            _logger.LogWarning(
                "Bill for customer {CustomerId} exceeds credit limit: {GrandTotal} {Currency}",
                customerId, bill.GrandTotal, currency);
        }

        await _billRepository.AddAsync(bill, cancellationToken);
        _logger.LogInformation(
            "Bill {BillId} generated for customer {CustomerId}: {Total} {Currency}",
            bill.Id, customerId, bill.GrandTotal, currency);

        return Result.Success(bill.Id);
    }

    private static decimal ProrateMonthlyFee(decimal monthlyFee, DateTime periodStart, DateTime periodEnd, DateTime activationDate)
    {
        var effectiveStart = activationDate > periodStart ? activationDate : periodStart;
        var totalDays = (periodEnd - periodStart).Days;
        if (totalDays <= 0)
        {
            return monthlyFee;
        }

        var activeDays = (periodEnd - effectiveStart).Days;
        if (activeDays <= 0)
        {
            return 0;
        }

        return monthlyFee * activeDays / totalDays;
    }

    private static decimal ApplyDiscountToBill(Bill bill, DiscountRule discount)
    {
        var discountAmount = bill.SubTotal * (discount.Percentage / 100m);
        if (discount.MaxDiscount.HasValue && discountAmount > discount.MaxDiscount.Value)
        {
            discountAmount = discount.MaxDiscount.Value;
        }

        return discountAmount;
    }
}
