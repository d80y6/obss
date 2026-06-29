using Mapster;
using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetAlertRules;

public sealed class GetAlertRulesQueryHandler : IRequestHandler<GetAlertRulesQuery, Result<IReadOnlyList<AuditAlertRuleDto>>>
{
    private readonly IAuditAlertRuleRepository _repository;

    public GetAlertRulesQueryHandler(IAuditAlertRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AuditAlertRuleDto>>> Handle(GetAlertRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _repository.GetAllAsync(cancellationToken);
        var result = rules.Adapt<List<AuditAlertRuleDto>>();
        return Result.Success<IReadOnlyList<AuditAlertRuleDto>>(result);
    }
}
