namespace Obss.ProductCatalog.Application.DTOs;

public sealed record BundledProductOfferingDto(
    Guid Id,
    Guid OfferId,
    Guid BundledOfferId,
    string? Name,
    int Quantity,
    string? ReferralType,
    OfferDto? BundledOffer);
