namespace Obss.Rating.Domain.DomainServices;

public sealed record PromotionResult(
    decimal Discount,
    Guid PromotionApplied,
    string PromotionName);
