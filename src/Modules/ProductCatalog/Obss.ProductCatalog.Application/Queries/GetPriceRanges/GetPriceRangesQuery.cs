using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetPriceRanges;

public sealed record GetPriceRangesQuery(Guid OfferPricingId) : IRequest<Result<List<PriceRangeDto>>>;
