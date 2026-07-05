using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.PatchOffer;

public sealed class PatchOfferCommandHandler : IRequestHandler<PatchOfferCommand, Result<OfferDto>>
{
    private readonly IOfferRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PatchOfferCommandHandler(IOfferRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OfferDto>> Handle(PatchOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _repository.GetByIdAsync(request.OfferId, cancellationToken);
        if (offer is null)
            return Result.Failure<OfferDto>(Error.NotFound("Offer", request.OfferId));

        offer.UpdateDetails(
            request.Name.HasValue ? request.Name.Value : offer.Name,
            request.Description.HasValue ? request.Description.Value : offer.Description,
            request.IsContract.HasValue ? request.IsContract.Value : offer.IsContract,
            request.ContractDurationMonths.HasValue ? request.ContractDurationMonths.Value : offer.ContractDurationMonths,
            request.BillingPeriod.HasValue ? request.BillingPeriod.Value : offer.BillingPeriod,
            request.TaxInclusive.HasValue ? request.TaxInclusive.Value : offer.TaxInclusive,
            request.SortOrder.HasValue ? request.SortOrder.Value : offer.SortOrder);

        if (request.ValidFrom.HasValue || request.ValidTo.HasValue)
        {
            offer.SetValidityPeriod(
                request.ValidFrom.HasValue ? request.ValidFrom.Value : offer.ValidFrom,
                request.ValidTo.HasValue ? request.ValidTo.Value : offer.ValidTo);
        }

        await _repository.UpdateAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(offer.Adapt<OfferDto>());
    }
}
