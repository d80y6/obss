using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.AddPriceRange;

public sealed class AddPriceRangeCommandHandler : IRequestHandler<AddPriceRangeCommand, Result<PriceRangeDto>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddPriceRangeCommandHandler(IOfferRepository offerRepository, IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PriceRangeDto>> Handle(AddPriceRangeCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithPricingsAndRangesAsync(request.OfferPricingId, cancellationToken);

        if (offer is null)
            return Result.Failure<PriceRangeDto>(Error.NotFound(nameof(Offer), request.OfferPricingId));

        var offerPricing = offer.OfferPricings.FirstOrDefault(op => op.Id == request.OfferPricingId);

        if (offerPricing is null)
            return Result.Failure<PriceRangeDto>(Error.NotFound(nameof(OfferPricing), request.OfferPricingId));

        var priceRange = new PriceRange(
            Guid.NewGuid(),
            request.OfferPricingId,
            request.MinQuantity,
            request.MaxQuantity,
            request.Price,
            request.IsActive);

        offerPricing.AddPriceRange(priceRange);

        await _offerRepository.UpdateAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(priceRange.Adapt<PriceRangeDto>());
    }
}
