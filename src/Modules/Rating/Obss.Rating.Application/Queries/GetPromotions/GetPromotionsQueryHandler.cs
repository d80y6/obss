using Mapster;
using MediatR;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Queries.GetPromotions;

public sealed class GetPromotionsQueryHandler : IRequestHandler<GetPromotionsQuery, Result<IReadOnlyList<PromotionDto>>>
{
    private readonly IPromotionRepository _promotionRepository;

    public GetPromotionsQueryHandler(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<Result<IReadOnlyList<PromotionDto>>> Handle(GetPromotionsQuery request, CancellationToken cancellationToken)
    {
        var promotions = await _promotionRepository.GetAllAsync(cancellationToken);
        return Result.Success(promotions.Adapt<IReadOnlyList<PromotionDto>>());
    }
}
