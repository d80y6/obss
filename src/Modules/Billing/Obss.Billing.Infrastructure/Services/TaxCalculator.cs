using Microsoft.Extensions.Logging;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.Services;
using Obss.Billing.Domain.ValueObjects;

namespace Obss.Billing.Infrastructure.Services;

public sealed class TaxCalculator : ITaxCalculator
{
    private readonly ITaxRuleRepository _taxRuleRepository;
    private readonly ILogger<TaxCalculator> _logger;

    public TaxCalculator(
        ITaxRuleRepository taxRuleRepository,
        ILogger<TaxCalculator> logger)
    {
        _taxRuleRepository = taxRuleRepository;
        _logger = logger;
    }

    public async Task<Bill> CalculateTaxesAsync(Bill bill, CancellationToken cancellationToken = default)
    {
        var allRules = await _taxRuleRepository.GetAllAsync(cancellationToken);
        var activeRules = allRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ToList();

        if (activeRules.Count == 0)
        {
            _logger.LogInformation("No active tax rules found for bill {BillId}.", bill.Id);
            return bill;
        }

        var lineCategories = bill.Lines
            .Where(l => l.LineType is not (LineType.Tax or LineType.Discount))
            .Select(l => l.LineType.ToString())
            .Distinct()
            .ToList();

        var applicableRules = new List<(TaxRule Rule, decimal Rate)>();

        foreach (var rule in activeRules)
        {
            foreach (var category in lineCategories)
            {
                if (!rule.IsApplicable(category, string.Empty))
                    continue;

                var effectiveRate = rule.TaxRate;

                var exemption = await _taxRuleRepository.GetExemptionAsync(
                    bill.CustomerId,
                    rule.Id,
                    cancellationToken);

                if (exemption is not null && exemption.IsValid())
                {
                    effectiveRate = exemption.GetEffectiveRate(effectiveRate);
                    _logger.LogInformation(
                        "Exemption applied for customer {CustomerId} on rule {TaxRuleId}: rate {OriginalRate} -> {EffectiveRate}",
                        bill.CustomerId,
                        rule.Id,
                        rule.TaxRate,
                        effectiveRate);
                }

                applicableRules.Add((rule, effectiveRate));
                break;
            }
        }

        var appliedRuleIds = new HashSet<Guid>();

        foreach (var (rule, effectiveRate) in applicableRules.OrderBy(r => r.Rule.Priority))
        {
            if (appliedRuleIds.Contains(rule.Id))
                continue;

            appliedRuleIds.Add(rule.Id);

            var taxableAmount = bill.SubTotal - bill.DiscountTotal;

            if (rule.IsCompound)
            {
                taxableAmount += bill.TaxTotal;
            }

            var taxAmount = rule.CalculateTax(taxableAmount);

            var taxLine = BillLine.CreateTaxLine(
                bill.Id,
                $"{rule.Name} - {rule.TaxCategory}",
                taxAmount,
                effectiveRate,
                bill.Currency,
                DateTime.UtcNow);

            bill.AddTaxLine(taxLine);

            _logger.LogInformation(
                "Tax {TaxName} applied to bill {BillId}: rate {TaxRate}, amount {TaxAmount}",
                rule.Name,
                bill.Id,
                effectiveRate,
                taxAmount);
        }

        bill.RecalculateTotalsWithTax();

        _logger.LogInformation(
            "Tax calculation completed for bill {BillId}. Tax total: {TaxTotal}",
            bill.Id,
            bill.TaxTotal);

        return bill;
    }
}
