using Obss.Rating.Domain.DomainServices;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.ValueObjects;

namespace Obss.Rating.Infrastructure.Services;

public sealed class PromotionEngine : IPromotionEngine
{
    public PromotionResult CalculateBestDiscount(IEnumerable<Promotion> promotions, BillLine line)
    {
        var applicable = GetApplicablePromotions(promotions, line)
            .OrderByDescending(p => p.Priority)
            .ThenByDescending(p => p.DiscountValue);

        var best = applicable.FirstOrDefault();

        if (best is null)
            return new PromotionResult(0, Guid.Empty, string.Empty);

        var discount = best.CalculateDiscount(line.Amount);
        return new PromotionResult(discount, best.Id, best.Name);
    }

    public IEnumerable<Promotion> GetApplicablePromotions(IEnumerable<Promotion> promotions, BillLine line)
    {
        return promotions
            .Where(p => p.IsApplicable(line.Amount, line.Quantity))
            .Where(p => EvaluateRules(p, line))
            .OrderBy(p => p.Priority);
    }

    private static bool EvaluateRules(Promotion promotion, BillLine line)
    {
        if (promotion.Rules.Count == 0)
            return true;

        var hasAnd = promotion.Rules.Any(r => r.Logic == RuleLogic.And);
        var hasOr = promotion.Rules.Any(r => r.Logic == RuleLogic.Or);

        if (hasAnd && hasOr)
        {
            var andResults = promotion.Rules
                .Where(r => r.Logic == RuleLogic.And)
                .All(r => EvaluateRule(r, line));

            var orResults = promotion.Rules
                .Where(r => r.Logic == RuleLogic.Or)
                .Any(r => EvaluateRule(r, line));

            return andResults && orResults;
        }

        if (hasAnd)
            return promotion.Rules.All(r => EvaluateRule(r, line));

        if (hasOr)
            return promotion.Rules.Any(r => EvaluateRule(r, line));

        return promotion.Rules.All(r => EvaluateRule(r, line));
    }

    private static bool EvaluateRule(PromotionRule rule, BillLine line)
    {
        return rule.RuleType switch
        {
            PromotionRuleType.MinimumAmount => EvaluateNumericOperator(rule.Operator, line.Amount, decimal.TryParse(rule.Value, out var val) ? val : 0),
            PromotionRuleType.ProductId => EvaluateStringOperator(rule.Operator, line.ProductId?.ToString(), rule.Value),
            PromotionRuleType.ServiceType => EvaluateStringOperator(rule.Operator, line.ProductId?.ToString(), rule.Value),
            _ => true
        };
    }

    private static bool EvaluateNumericOperator(RuleOperator op, decimal left, decimal right)
    {
        return op switch
        {
            RuleOperator.Equals => left == right,
            RuleOperator.GreaterThan => left > right,
            RuleOperator.LessThan => left < right,
            _ => true
        };
    }

    private static bool EvaluateStringOperator(RuleOperator op, string? left, string right)
    {
        if (left is null)
            return false;

        return op switch
        {
            RuleOperator.Equals => string.Equals(left, right, StringComparison.OrdinalIgnoreCase),
            RuleOperator.Contains => left.Contains(right, StringComparison.OrdinalIgnoreCase),
            RuleOperator.In => right.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(left, StringComparer.OrdinalIgnoreCase),
            _ => true
        };
    }
}
