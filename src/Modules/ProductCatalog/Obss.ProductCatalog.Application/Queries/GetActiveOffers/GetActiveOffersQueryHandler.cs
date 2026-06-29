using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetActiveOffers;

public sealed class GetActiveOffersQueryHandler : IRequestHandler<GetActiveOffersQuery, Result<IReadOnlyList<OfferDto>>>
{
    private readonly IOfferRepository _offerRepository;

    public GetActiveOffersQueryHandler(IOfferRepository offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<Result<IReadOnlyList<OfferDto>>> Handle(GetActiveOffersQuery request, CancellationToken cancellationToken)
    {
        var offers = await _offerRepository.GetActiveOffersAsync(request.OfferType, cancellationToken);

        var result = offers.Adapt<List<OfferDto>>();
        return Result.Success<IReadOnlyList<OfferDto>>(result);
    }
}
