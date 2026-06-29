namespace Obss.Rating.Application.DTOs;

public sealed record PromotionDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string PromotionType,
    decimal DiscountValue,
    string Currency,
    int? MinQuantity,
    int? MaxQuantity,
    DateTime ValidFrom,
    DateTime? ValidTo,
    bool IsActive,
    bool IsStackable,
    int Priority,
    string? Code,
    int? MaxRedemptions,
    int CurrentRedemptions,
    DateTime CreatedAt,
    List<PromotionRuleDto> Rules);
