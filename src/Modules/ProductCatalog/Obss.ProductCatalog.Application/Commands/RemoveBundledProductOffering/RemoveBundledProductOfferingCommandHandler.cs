using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.RemoveBundledProductOffering;

public sealed class RemoveBundledProductOfferingCommandHandler : IRequestHandler<RemoveBundledProductOfferingCommand, Result>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveBundledProductOfferingCommandHandler(IOfferRepository offerRepository, IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveBundledProductOfferingCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithBundledOfferingsAsync(request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure(Error.NotFound(nameof(Offer), request.OfferId));

        var bundledOffering = offer.BundledOfferings.FirstOrDefault(b => b.Id == request.BundledOfferingId);

        if (bundledOffering is null)
            return Result.Failure(Error.NotFound("BundledProductOffering", request.BundledOfferingId));

        offer.RemoveBundledOffering(bundledOffering);

        await _offerRepository.UpdateAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
