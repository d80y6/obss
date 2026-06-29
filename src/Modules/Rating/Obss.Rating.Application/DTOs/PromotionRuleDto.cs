namespace Obss.Rating.Application.DTOs;

public sealed record PromotionRuleDto(
    Guid Id,
    Guid PromotionId,
    string RuleType,
    string Operator,
    string Value,
    string Logic);
