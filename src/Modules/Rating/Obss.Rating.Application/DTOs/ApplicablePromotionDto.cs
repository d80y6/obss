namespace Obss.Rating.Application.DTOs;

public sealed record ApplicablePromotionDto(
    Guid PromotionId,
    string PromotionName,
    string PromotionType,
    decimal DiscountValue,
    decimal CalculatedDiscount,
    string Currency,
    int Priority);
