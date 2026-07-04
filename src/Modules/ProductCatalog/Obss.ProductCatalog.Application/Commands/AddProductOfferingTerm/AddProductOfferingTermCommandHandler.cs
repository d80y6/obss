using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.AddProductOfferingTerm;

public sealed class AddProductOfferingTermCommandHandler : IRequestHandler<AddProductOfferingTermCommand, Result<ProductOfferingTermDto>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddProductOfferingTermCommandHandler(IOfferRepository offerRepository, IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductOfferingTermDto>> Handle(AddProductOfferingTermCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithTermsAsync(request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure<ProductOfferingTermDto>(Error.NotFound(nameof(Offer), request.OfferId));

        var term = new ProductOfferingTerm(
            Guid.NewGuid(),
            request.OfferId,
            request.Name,
            request.Description,
            request.Duration,
            request.DurationUnit,
            request.TermType,
            request.ValidFrom,
            request.ValidTo);

        offer.AddTerm(term);

        await _offerRepository.UpdateAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(term.Adapt<ProductOfferingTermDto>());
    }
}
