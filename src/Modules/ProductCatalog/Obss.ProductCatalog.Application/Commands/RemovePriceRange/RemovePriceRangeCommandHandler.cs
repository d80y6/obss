using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.RemovePriceRange;

public sealed class RemovePriceRangeCommandHandler : IRequestHandler<RemovePriceRangeCommand, Result>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemovePriceRangeCommandHandler(IOfferRepository offerRepository, IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemovePriceRangeCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithPricingsAndRangesAsync(request.OfferPricingId, cancellationToken);

        if (offer is null)
            return Result.Failure(Error.NotFound(nameof(Offer), request.OfferPricingId));

        var offerPricing = offer.OfferPricings.FirstOrDefault(op => op.Id == request.OfferPricingId);

        if (offerPricing is null)
            return Result.Failure(Error.NotFound(nameof(OfferPricing), request.OfferPricingId));

        var priceRange = offerPricing.PriceRanges.FirstOrDefault(pr => pr.Id == request.PriceRangeId);

        if (priceRange is null)
            return Result.Failure(Error.NotFound(nameof(PriceRange), request.PriceRangeId));

        offerPricing.RemovePriceRange(priceRange);

        await _offerRepository.UpdateAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
