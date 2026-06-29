using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Application.DTOs;

public sealed record OfferPricingDto(
    Guid Id,
    Guid OfferId,
    PricingType PricingType,
    string Currency,
    decimal RecurringPrice,
    decimal OneTimePrice,
    decimal UsagePrice,
    string? UnitOfMeasure,
    int? MinQuantity,
    int? MaxQuantity,
    bool IsActive);
