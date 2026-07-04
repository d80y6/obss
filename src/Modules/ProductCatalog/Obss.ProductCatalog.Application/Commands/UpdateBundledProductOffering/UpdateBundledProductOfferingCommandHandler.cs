using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateBundledProductOffering;

public sealed class UpdateBundledProductOfferingCommandHandler : IRequestHandler<UpdateBundledProductOfferingCommand, Result<BundledProductOfferingDto>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBundledProductOfferingCommandHandler(IOfferRepository offerRepository, IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BundledProductOfferingDto>> Handle(UpdateBundledProductOfferingCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithBundledOfferingsAsync(request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure<BundledProductOfferingDto>(Error.NotFound(nameof(Offer), request.OfferId));

        var bundledOffering = offer.BundledOfferings.FirstOrDefault(b => b.Id == request.BundledOfferingId);

        if (bundledOffering is null)
            return Result.Failure<BundledProductOfferingDto>(Error.NotFound("BundledProductOffering", request.BundledOfferingId));

        bundledOffering.Update(request.Name, request.Quantity, request.ReferralType);

        await _offerRepository.UpdateAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(bundledOffering.Adapt<BundledProductOfferingDto>());
    }
}
