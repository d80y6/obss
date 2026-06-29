using Mapster;
using MediatR;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetTaxRules;

public sealed class GetTaxRulesQueryHandler : IRequestHandler<GetTaxRulesQuery, Result<IReadOnlyList<TaxRuleDto>>>
{
    private readonly ITaxRuleRepository _taxRuleRepository;

    public GetTaxRulesQueryHandler(ITaxRuleRepository taxRuleRepository)
    {
        _taxRuleRepository = taxRuleRepository;
    }

    public async Task<Result<IReadOnlyList<TaxRuleDto>>> Handle(GetTaxRulesQuery request, CancellationToken cancellationToken)
    {
        var taxRules = await _taxRuleRepository.GetApplicableRulesAsync(
            request.Category ?? string.Empty,
            request.Country ?? string.Empty,
            cancellationToken);

        return Result.Success<IReadOnlyList<TaxRuleDto>>(taxRules.Adapt<List<TaxRuleDto>>());
    }
}
