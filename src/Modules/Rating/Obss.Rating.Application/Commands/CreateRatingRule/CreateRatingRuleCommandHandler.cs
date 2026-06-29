using Mapster;
using MediatR;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.DTOs;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.CreateRatingRule;

public sealed class CreateRatingRuleCommandHandler : IRequestHandler<CreateRatingRuleCommand, Result<RatingRuleDto>>
{
    private readonly IRatingRuleRepository _ratingRuleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public CreateRatingRuleCommandHandler(
        IRatingRuleRepository ratingRuleRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _ratingRuleRepository = ratingRuleRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<RatingRuleDto>> Handle(CreateRatingRuleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
            return Result.Failure<RatingRuleDto>(Error.Unauthorized("Tenant context is required."));

        if (!Enum.TryParse<RatingRuleType>(request.RuleType, true, out var ruleType))
            return Result.Failure<RatingRuleDto>(Error.Validation($"Invalid rule type: '{request.RuleType}'. Must be Usage, Time, Volume, or Flat."));

        var rule = RatingRule.Create(
            tenantId,
            request.Name,
            request.Description,
            ruleType,
            request.ProductId,
            request.OfferId,
            request.Priority);

        foreach (var tierDto in request.Tiers)
        {
            rule.AddTier(new RatingTier(tierDto.FromUnit, tierDto.ToUnit, tierDto.Rate, tierDto.Description));
        }

        await _ratingRuleRepository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.Adapt<RatingRuleDto>());
    }
}
