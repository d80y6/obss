using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Application.DTOs;

public sealed record OfferDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    OfferType OfferType,
    bool IsActive,
    bool IsContract,
    int? ContractDurationMonths,
    BillingPeriod? BillingPeriod,
    bool TaxInclusive,
    int SortOrder,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<OfferPricingDto> Pricings,
    List<OfferDiscountDto> Discounts,
    List<ProductOfferingTermDto> Terms);

public sealed record OfferDiscountDto(
    Guid Id,
    DiscountType DiscountType,
    decimal DiscountValue,
    int? DiscountPeriodMonths,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    bool IsActive,
    string? Description);
