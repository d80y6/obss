using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetBundledProductOfferings;

public sealed class GetBundledProductOfferingsQueryHandler : IRequestHandler<GetBundledProductOfferingsQuery, Result<List<BundledProductOfferingDto>>>
{
    private readonly IOfferRepository _offerRepository;

    public GetBundledProductOfferingsQueryHandler(IOfferRepository offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<Result<List<BundledProductOfferingDto>>> Handle(GetBundledProductOfferingsQuery request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithBundledOfferingsAsync(request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure<List<BundledProductOfferingDto>>(Error.NotFound(nameof(Offer), request.OfferId));

        return Result.Success(offer.BundledOfferings.Adapt<List<BundledProductOfferingDto>>());
    }
}
