namespace Obss.Billing.Application.DTOs;

public sealed record TaxRuleDto(
    Guid Id,
    string TenantId,
    string Name,
    string Description,
    decimal TaxRate,
    string TaxType,
    string TaxCategory,
    string Country,
    string Region,
    bool IsCompound,
    bool IsActive,
    int Priority,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    DateTime CreatedAt,
    string? ExternalId);
