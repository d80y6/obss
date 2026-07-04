using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.AddBundledProductOffering;

public sealed class AddBundledProductOfferingCommandHandler : IRequestHandler<AddBundledProductOfferingCommand, Result<BundledProductOfferingDto>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddBundledProductOfferingCommandHandler(IOfferRepository offerRepository, IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BundledProductOfferingDto>> Handle(AddBundledProductOfferingCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithBundledOfferingsAsync(request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure<BundledProductOfferingDto>(Error.NotFound(nameof(Offer), request.OfferId));

        var bundledOffer = await _offerRepository.GetByIdAsync(request.BundledOfferId, cancellationToken);

        if (bundledOffer is null)
            return Result.Failure<BundledProductOfferingDto>(Error.NotFound(nameof(Offer), request.BundledOfferId));

        var bundledOffering = new BundledProductOffering(
            Guid.NewGuid(),
            request.OfferId,
            request.BundledOfferId,
            request.Name,
            request.Quantity,
            request.ReferralType);

        offer.AddBundledOffering(bundledOffering);

        await _offerRepository.UpdateAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(bundledOffering.Adapt<BundledProductOfferingDto>());
    }
}
