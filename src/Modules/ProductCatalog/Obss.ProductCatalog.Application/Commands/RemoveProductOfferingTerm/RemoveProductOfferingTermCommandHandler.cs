using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.RemoveProductOfferingTerm;

public sealed class RemoveProductOfferingTermCommandHandler : IRequestHandler<RemoveProductOfferingTermCommand, Result<Unit>>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveProductOfferingTermCommandHandler(IOfferRepository offerRepository, IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(RemoveProductOfferingTermCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdWithTermsAsync(request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure<Unit>(Error.NotFound(nameof(Offer), request.OfferId));

        var term = offer.Terms.FirstOrDefault(t => t.Id == request.TermId);

        if (term is null)
            return Result.Failure<Unit>(Error.NotFound("ProductOfferingTerm", request.TermId));

        offer.RemoveTerm(term);

        await _offerRepository.UpdateAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}
