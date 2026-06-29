using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.ValueObjects;

public sealed class SegmentCriteria : ValueObject
{
    private SegmentCriteria() { }

    public SegmentCriteria(List<RuleGroup> ruleGroups)
    {
        RuleGroups = ruleGroups;
    }

    public List<RuleGroup> RuleGroups { get; private set; } = [];

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return RuleGroups;
    }
}

public sealed class RuleGroup
{
    public RuleGroup() { }

    public RuleGroup(string conjunction, List<Rule> rules)
    {
        Conjunction = conjunction;
        Rules = rules;
    }

    public string Conjunction { get; set; } = "And";
    public List<Rule> Rules { get; set; } = [];
}

public sealed class Rule
{
    public Rule() { }

    public Rule(string field, string @operator, string value)
    {
        Field = field;
        Operator = @operator;
        Value = value;
    }

    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
