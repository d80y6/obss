using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.DeleteOffer;

public sealed class DeleteOfferCommandHandler : IRequestHandler<DeleteOfferCommand, Result>
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOfferCommandHandler(IOfferRepository offerRepository, IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdAsync(request.OfferId, cancellationToken);

        if (offer is null)
            return Result.Failure(Error.NotFound(nameof(Offer), request.OfferId));

        await _offerRepository.DeleteAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
