using MediatR;
using Obss.Rating.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.DeactivatePromotion;

public sealed class DeactivatePromotionCommandHandler : IRequestHandler<DeactivatePromotionCommand, Result>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePromotionCommandHandler(
        IPromotionRepository promotionRepository,
        IUnitOfWork unitOfWork)
    {
        _promotionRepository = promotionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await _promotionRepository.GetByIdAsync(request.PromotionId, cancellationToken);

        if (promotion is null)
            return Result.Failure(Error.NotFound("Promotion", request.PromotionId));

        promotion.Deactivate();

        await _promotionRepository.UpdateAsync(promotion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
