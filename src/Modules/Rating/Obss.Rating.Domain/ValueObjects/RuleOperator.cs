namespace Obss.Rating.Domain.ValueObjects;

public enum RuleOperator
{
    Equals,
    GreaterThan,
    LessThan,
    Contains,
    In
}

public enum RuleLogic
{
    And,
    Or
}
