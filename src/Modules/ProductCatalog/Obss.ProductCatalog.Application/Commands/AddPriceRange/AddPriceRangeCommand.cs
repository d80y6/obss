using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.AddPriceRange;

public sealed record AddPriceRangeCommand(
    Guid OfferPricingId,
    int MinQuantity,
    int? MaxQuantity,
    decimal Price,
    bool IsActive) : IRequest<Result<PriceRangeDto>>;
