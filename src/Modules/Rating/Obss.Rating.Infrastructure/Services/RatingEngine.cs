using Obss.Rating.Domain.DomainServices;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.ValueObjects;

namespace Obss.Rating.Infrastructure.Services;

public sealed class RatingEngine : IRatingEngine
{
    public RatedUsageResult Rate(UsageRecord record, RatingRule rule)
    {
        var amount = rule.RuleType switch
        {
            RatingRuleType.Flat => CalculateFlatRate(rule),
            RatingRuleType.Usage => CalculateUsageRate(record, rule),
            RatingRuleType.Time => CalculateTimeRate(record, rule),
            RatingRuleType.Volume => CalculateVolumeRate(record, rule),
            _ => throw new ArgumentOutOfRangeException(nameof(rule), rule.RuleType, $"Unsupported rule type: {rule.RuleType}")
        };

        return new RatedUsageResult(record.Id, amount, record.Currency, rule.Id);
    }

    public IEnumerable<RatedUsageResult> RateBatch(IEnumerable<UsageRecord> records, RatingRule rule)
    {
        return records.Select(record => Rate(record, rule));
    }

    private static decimal CalculateFlatRate(RatingRule rule)
    {
        var tier = rule.Tiers.FirstOrDefault();
        return tier?.Rate ?? 0;
    }

    private static decimal CalculateUsageRate(UsageRecord record, RatingRule rule)
    {
        var units = record.Duration;
        return ApplyTieredRating(units, rule);
    }

    private static decimal CalculateTimeRate(UsageRecord record, RatingRule rule)
    {
        var minutes = Math.Max(1, (long)Math.Ceiling((record.EndTime - record.StartTime).TotalMinutes));
        return ApplyTieredRating(minutes, rule);
    }

    private static decimal CalculateVolumeRate(UsageRecord record, RatingRule rule)
    {
        var megabytes = Math.Max(1, record.Volume / (1024 * 1024));
        return ApplyTieredRating(megabytes, rule);
    }

    private static decimal ApplyTieredRating(long units, RatingRule rule)
    {
        if (rule.Tiers.Count == 0)
            return 0;

        var orderedTiers = rule.Tiers.OrderBy(t => t.FromUnit).ToList();
        var totalCost = 0m;
        var remaining = units;

        foreach (var tier in orderedTiers)
        {
            if (remaining <= 0)
                break;

            var tierUnits = tier.ToUnit.HasValue
                ? Math.Min(remaining, tier.ToUnit.Value - tier.FromUnit + 1)
                : remaining;

            totalCost += tierUnits * tier.Rate;
            remaining -= tierUnits;
        }

        return totalCost;
    }
}
