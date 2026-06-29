namespace Obss.Rating.Application.DTOs;

public sealed record RatingRuleDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string RuleType,
    Guid? ProductId,
    Guid? OfferId,
    bool IsActive,
    int Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<RatingTierDto> Tiers);

public sealed record RatingTierDto(
    int FromUnit,
    int? ToUnit,
    decimal Rate,
    string? Description);
