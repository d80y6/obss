using Mapster;
using MediatR;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Queries.GetRules;

public sealed class GetRulesQueryHandler : IRequestHandler<GetRulesQuery, Result<IReadOnlyList<RatingRuleDto>>>
{
    private readonly IRatingRuleRepository _ruleRepository;

    public GetRulesQueryHandler(IRatingRuleRepository ruleRepository)
    {
        _ruleRepository = ruleRepository;
    }

    public async Task<Result<IReadOnlyList<RatingRuleDto>>> Handle(GetRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _ruleRepository.GetAllAsync(cancellationToken);
        return Result.Success(rules.Adapt<IReadOnlyList<RatingRuleDto>>());
    }
}
