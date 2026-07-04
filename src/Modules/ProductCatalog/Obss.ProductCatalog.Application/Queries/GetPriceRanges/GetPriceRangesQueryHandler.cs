using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetPriceRanges;

public sealed class GetPriceRangesQueryHandler : IRequestHandler<GetPriceRangesQuery, Result<List<PriceRangeDto>>>
{
    private readonly IOfferRepository _offerRepository;

    public GetPriceRangesQueryHandler(IOfferRepository offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<Result<List<PriceRangeDto>>> Handle(GetPriceRangesQuery request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithPricingsAndRangesAsync(request.OfferPricingId, cancellationToken);

        if (offer is null)
            return Result.Failure<List<PriceRangeDto>>(Error.NotFound(nameof(Offer), request.OfferPricingId));

        var offerPricing = offer.OfferPricings.FirstOrDefault(op => op.Id == request.OfferPricingId);

        if (offerPricing is null)
            return Result.Failure<List<PriceRangeDto>>(Error.NotFound(nameof(OfferPricing), request.OfferPricingId));

        return Result.Success(offerPricing.PriceRanges.Adapt<List<PriceRangeDto>>());
    }
}
