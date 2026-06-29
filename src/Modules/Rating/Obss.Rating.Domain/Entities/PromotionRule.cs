using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Rating.Domain.Entities;

public class PromotionRule : Entity<Guid>
{
    private PromotionRule() { }

    public PromotionRule(
        Guid id,
        Guid promotionId,
        PromotionRuleType ruleType,
        RuleOperator @operator,
        string value,
        RuleLogic logic)
        : base(id)
    {
        PromotionId = promotionId;
        RuleType = ruleType;
        Operator = @operator;
        Value = value;
        Logic = logic;
    }

    public Guid PromotionId { get; private set; }
    public PromotionRuleType RuleType { get; private set; }
    public RuleOperator Operator { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public RuleLogic Logic { get; private set; }
}
