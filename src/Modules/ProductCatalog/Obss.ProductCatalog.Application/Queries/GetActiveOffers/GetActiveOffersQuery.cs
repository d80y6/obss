using MediatR;
using Obss.ProductCatalog.Application.Contracts;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetActiveOffers;

public sealed record GetActiveOffersQuery(
    OfferType? OfferType,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedResult<OfferDto>>>;
