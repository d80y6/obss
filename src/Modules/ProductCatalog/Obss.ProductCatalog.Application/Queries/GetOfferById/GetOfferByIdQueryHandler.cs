using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetOfferById;

public sealed class GetOfferByIdQueryHandler : IRequestHandler<GetOfferByIdQuery, Result<OfferDto>>
{
    private readonly IOfferRepository _offerRepository;

    public GetOfferByIdQueryHandler(IOfferRepository offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<Result<OfferDto>> Handle(GetOfferByIdQuery request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithPricingsAsync(request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure<OfferDto>(Error.NotFound(nameof(Offer), request.OfferId));

        return Result.Success(offer.Adapt<OfferDto>());
    }
}
