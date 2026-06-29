using MediatR;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.DTOs;
using Obss.Rating.Domain.DomainServices;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Queries.GetApplicablePromotions;

public sealed class GetApplicablePromotionsQueryHandler : IRequestHandler<GetApplicablePromotionsQuery, Result<IReadOnlyList<ApplicablePromotionDto>>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IPromotionEngine _promotionEngine;

    public GetApplicablePromotionsQueryHandler(
        IPromotionRepository promotionRepository,
        IPromotionEngine promotionEngine)
    {
        _promotionRepository = promotionRepository;
        _promotionEngine = promotionEngine;
    }

    public async Task<Result<IReadOnlyList<ApplicablePromotionDto>>> Handle(GetApplicablePromotionsQuery request, CancellationToken cancellationToken)
    {
        var promotions = await _promotionRepository.GetActivePromotionsAsync(cancellationToken);

        var line = new BillLine(request.Amount ?? 0, request.Quantity ?? 0, request.ProductId, request.SubscriptionId);
        var applicable = _promotionEngine.GetApplicablePromotions(promotions, line).ToList();

        var results = applicable.Select(p =>
        {
            var discount = p.CalculateDiscount(request.Amount ?? 0);
            return new ApplicablePromotionDto(
                p.Id,
                p.Name,
                p.PromotionType.ToString(),
                p.DiscountValue,
                discount,
                p.Currency,
                p.Priority);
        }).ToList();

        return Result.Success<IReadOnlyList<ApplicablePromotionDto>>(results);
    }
}
