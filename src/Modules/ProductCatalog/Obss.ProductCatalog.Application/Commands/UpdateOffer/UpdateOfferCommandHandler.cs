using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateOffer;

public sealed class UpdateOfferCommandHandler : IRequestHandler<UpdateOfferCommand, Result<OfferDto>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOfferCommandHandler(IOfferRepository offerRepository, IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OfferDto>> Handle(UpdateOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdAsync(request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure<OfferDto>(Error.NotFound(nameof(Offer), request.OfferId));

        offer.UpdateDetails(
            request.Name,
            request.Description,
            request.IsContract,
            request.ContractDurationMonths,
            request.BillingPeriod,
            request.TaxInclusive,
            request.SortOrder);

        if (request.ValidFrom.HasValue || request.ValidTo.HasValue)
        {
            offer.SetValidityPeriod(request.ValidFrom, request.ValidTo);
        }

        await _offerRepository.UpdateAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(offer.Adapt<OfferDto>());
    }
}
