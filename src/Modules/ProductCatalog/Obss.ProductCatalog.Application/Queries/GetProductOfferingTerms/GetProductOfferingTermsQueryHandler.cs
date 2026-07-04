using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductOfferingTerms;

public sealed class GetProductOfferingTermsQueryHandler : IRequestHandler<GetProductOfferingTermsQuery, Result<List<ProductOfferingTermDto>>>
{
    private readonly IOfferRepository _offerRepository;

    public GetProductOfferingTermsQueryHandler(IOfferRepository offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<Result<List<ProductOfferingTermDto>>> Handle(GetProductOfferingTermsQuery request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithTermsAsync(request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure<List<ProductOfferingTermDto>>(Error.NotFound(nameof(Offer), request.OfferId));

        return Result.Success(offer.Terms.Adapt<List<ProductOfferingTermDto>>());
    }
}
