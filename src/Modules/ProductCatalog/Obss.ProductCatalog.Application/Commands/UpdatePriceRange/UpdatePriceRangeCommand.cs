using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdatePriceRange;

public sealed record UpdatePriceRangeCommand(
    Guid OfferPricingId,
    Guid PriceRangeId,
    int MinQuantity,
    int? MaxQuantity,
    decimal Price,
    bool IsActive) : IRequest<Result<PriceRangeDto>>;
