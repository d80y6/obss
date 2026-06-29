using Mapster;
using MediatR;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetCustomerTaxExemptions;

public sealed class GetCustomerTaxExemptionsQueryHandler : IRequestHandler<GetCustomerTaxExemptionsQuery, Result<IReadOnlyList<TaxExemptionDto>>>
{
    private readonly ITaxRuleRepository _taxRuleRepository;

    public GetCustomerTaxExemptionsQueryHandler(ITaxRuleRepository taxRuleRepository)
    {
        _taxRuleRepository = taxRuleRepository;
    }

    public async Task<Result<IReadOnlyList<TaxExemptionDto>>> Handle(GetCustomerTaxExemptionsQuery request, CancellationToken cancellationToken)
    {
        var exemptions = await _taxRuleRepository.GetCustomerExemptionsAsync(request.CustomerId, cancellationToken);

        return Result.Success<IReadOnlyList<TaxExemptionDto>>(exemptions.Adapt<List<TaxExemptionDto>>());
    }
}
