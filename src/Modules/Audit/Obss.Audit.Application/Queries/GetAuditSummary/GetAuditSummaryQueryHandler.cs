using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetAuditSummary;

public sealed class GetAuditSummaryQueryHandler : IRequestHandler<GetAuditSummaryQuery, Result<AuditSummaryDto>>
{
    private readonly IAuditEntryRepository _repository;
    private readonly ICurrentTenant _currentTenant;

    public GetAuditSummaryQueryHandler(IAuditEntryRepository repository, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<AuditSummaryDto>> Handle(GetAuditSummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId;

        var byAction = await _repository.GetCountByActionAsync(tenantId, cancellationToken);
        var byEntityType = await _repository.GetCountByEntityTypeAsync(tenantId, cancellationToken);

        return Result.Success(new AuditSummaryDto(
            byAction,
            byEntityType,
            byAction.Values.Sum(),
            byEntityType.Values.Sum()));
    }
}
