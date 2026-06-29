using MediatR;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.DTOs;
using Obss.Rating.Domain.DomainServices;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.ApplyPromotion;

public sealed class ApplyPromotionCommandHandler : IRequestHandler<ApplyPromotionCommand, Result<ApplicablePromotionDto>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IPromotionEngine _promotionEngine;
    private readonly IUnitOfWork _unitOfWork;

    public ApplyPromotionCommandHandler(
        IPromotionRepository promotionRepository,
        IPromotionEngine promotionEngine,
        IUnitOfWork unitOfWork)
    {
        _promotionRepository = promotionRepository;
        _promotionEngine = promotionEngine;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ApplicablePromotionDto>> Handle(ApplyPromotionCommand request, CancellationToken cancellationToken)
    {
        Promotion? promotion = null;

        if (!string.IsNullOrWhiteSpace(request.PromotionCode))
        {
            promotion = await _promotionRepository.GetByCodeAsync(request.PromotionCode, cancellationToken);
        }
        else if (request.PromotionId.HasValue)
        {
            promotion = await _promotionRepository.GetByIdAsync(request.PromotionId.Value, cancellationToken);
        }

        if (promotion is null)
            return Result.Failure<ApplicablePromotionDto>(Error.NotFound("Promotion", request.PromotionCode ?? request.PromotionId?.ToString() ?? "unknown"));

        if (!promotion.IsValid())
            return Result.Failure<ApplicablePromotionDto>(Error.Validation("Promotion is not valid or has expired."));

        var line = new BillLine(request.Amount, request.Quantity, request.ProductId, request.SubscriptionId);
        var applicable = _promotionEngine.GetApplicablePromotions([promotion], line).ToList();

        if (applicable.Count == 0)
            return Result.Failure<ApplicablePromotionDto>(Error.Validation("Promotion is not applicable to this line."));

        var result = _promotionEngine.CalculateBestDiscount([promotion], line);

        promotion.ApplyPromotion(result.Discount, promotion.Currency, request.SubscriptionId, null);

        await _promotionRepository.UpdateAsync(promotion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ApplicablePromotionDto(
            promotion.Id,
            promotion.Name,
            promotion.PromotionType.ToString(),
            promotion.DiscountValue,
            result.Discount,
            promotion.Currency,
            promotion.Priority));
    }
}
