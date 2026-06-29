using Mapster;
using MediatR;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.DTOs;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.CreatePromotion;

public sealed class CreatePromotionCommandHandler : IRequestHandler<CreatePromotionCommand, Result<PromotionDto>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public CreatePromotionCommandHandler(
        IPromotionRepository promotionRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _promotionRepository = promotionRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<PromotionDto>> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
            return Result.Failure<PromotionDto>(Error.Unauthorized("Tenant context is required."));

        if (!Enum.TryParse<PromotionType>(request.PromotionType, true, out var promotionType))
            return Result.Failure<PromotionDto>(Error.Validation($"Invalid promotion type: '{request.PromotionType}'. Must be Percentage, FixedAmount, FreePeriod, Bundle, or Volume."));

        var promotion = Promotion.Create(
            tenantId,
            request.Name,
            request.Description,
            promotionType,
            request.DiscountValue,
            request.Currency,
            request.MinQuantity,
            request.MaxQuantity,
            request.ValidFrom,
            request.ValidTo,
            request.IsStackable,
            request.Priority,
            request.Code,
            request.MaxRedemptions);

        foreach (var ruleDto in request.Rules)
        {
            if (!Enum.TryParse<PromotionRuleType>(ruleDto.RuleType, true, out var ruleType))
                return Result.Failure<PromotionDto>(Error.Validation($"Invalid rule type: '{ruleDto.RuleType}'."));

            if (!Enum.TryParse<RuleOperator>(ruleDto.Operator, true, out var @operator))
                return Result.Failure<PromotionDto>(Error.Validation($"Invalid operator: '{ruleDto.Operator}'."));

            if (!Enum.TryParse<RuleLogic>(ruleDto.Logic, true, out var logic))
                return Result.Failure<PromotionDto>(Error.Validation($"Invalid logic: '{ruleDto.Logic}'."));

            var rule = new PromotionRule(
                Guid.NewGuid(),
                promotion.Id,
                ruleType,
                @operator,
                ruleDto.Value,
                logic);

            promotion.AddRule(rule);
        }

        await _promotionRepository.AddAsync(promotion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(promotion.Adapt<PromotionDto>());
    }
}
