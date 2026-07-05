using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.Contracts;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetActiveOffers;

public sealed class GetActiveOffersQueryHandler : IRequestHandler<GetActiveOffersQuery, Result<PaginatedResult<OfferDto>>>
{
    private readonly IOfferRepository _offerRepository;

    public GetActiveOffersQueryHandler(IOfferRepository offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<Result<PaginatedResult<OfferDto>>> Handle(GetActiveOffersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _offerRepository.GetFilteredAsync(
            request.OfferType,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = new PaginatedResult<OfferDto>(
            items.Adapt<List<OfferDto>>(),
            totalCount);

        return Result.Success(result);
    }
}
