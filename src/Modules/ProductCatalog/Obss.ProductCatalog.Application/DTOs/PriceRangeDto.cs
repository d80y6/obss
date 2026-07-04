namespace Obss.ProductCatalog.Application.DTOs;

public sealed record PriceRangeDto(
    Guid Id,
    Guid OfferPricingId,
    int MinQuantity,
    int? MaxQuantity,
    decimal Price,
    bool IsActive);
