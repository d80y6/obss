using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateOfferPricing;

public sealed class UpdateOfferPricingCommandHandler : IRequestHandler<UpdateOfferPricingCommand, Result<OfferPricingDto>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOfferPricingCommandHandler(IOfferRepository offerRepository, IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OfferPricingDto>> Handle(UpdateOfferPricingCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithPricingsAsync(request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure<OfferPricingDto>(Error.NotFound(nameof(Domain.Domain.Entities.Offer), request.OfferId));

        var pricing = offer.OfferPricings.FirstOrDefault(p => p.Id == request.OfferPricingId);

        if (pricing is null)
            return Result.Failure<OfferPricingDto>(Error.NotFound("OfferPricing", request.OfferPricingId));

        pricing.UpdatePricing(
            request.PricingType,
            request.Currency,
            request.RecurringPrice,
            request.OneTimePrice,
            request.UsagePrice,
            request.UnitOfMeasure,
            request.MinQuantity,
            request.MaxQuantity,
            request.IsActive);

        await _offerRepository.UpdateAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(pricing.Adapt<OfferPricingDto>());
    }
}
