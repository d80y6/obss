using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetActiveOffers;

public sealed record GetActiveOffersQuery(
    OfferType? OfferType) : IRequest<Result<IReadOnlyList<OfferDto>>>;
