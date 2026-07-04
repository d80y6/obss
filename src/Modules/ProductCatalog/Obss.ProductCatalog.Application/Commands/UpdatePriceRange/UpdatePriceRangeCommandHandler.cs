using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdatePriceRange;

public sealed class UpdatePriceRangeCommandHandler : IRequestHandler<UpdatePriceRangeCommand, Result<PriceRangeDto>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePriceRangeCommandHandler(IOfferRepository offerRepository, IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PriceRangeDto>> Handle(UpdatePriceRangeCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithPricingsAndRangesAsync(request.OfferPricingId, cancellationToken);

        if (offer is null)
            return Result.Failure<PriceRangeDto>(Error.NotFound(nameof(Offer), request.OfferPricingId));

        var offerPricing = offer.OfferPricings.FirstOrDefault(op => op.Id == request.OfferPricingId);

        if (offerPricing is null)
            return Result.Failure<PriceRangeDto>(Error.NotFound(nameof(OfferPricing), request.OfferPricingId));

        var priceRange = offerPricing.PriceRanges.FirstOrDefault(pr => pr.Id == request.PriceRangeId);

        if (priceRange is null)
            return Result.Failure<PriceRangeDto>(Error.NotFound(nameof(PriceRange), request.PriceRangeId));

        priceRange.Update(request.MinQuantity, request.MaxQuantity, request.Price, request.IsActive);

        await _offerRepository.UpdateAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(priceRange.Adapt<PriceRangeDto>());
    }
}
