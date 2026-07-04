using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.RemovePriceRange;

public sealed record RemovePriceRangeCommand(
    Guid OfferPricingId,
    Guid PriceRangeId) : IRequest<Result>;
