using Mapster;
using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetUnacknowledgedAlerts;

public sealed class GetUnacknowledgedAlertsQueryHandler : IRequestHandler<GetUnacknowledgedAlertsQuery, Result<IReadOnlyList<AuditAlertDto>>>
{
    private readonly IAuditAlertRepository _repository;
    private readonly ICurrentTenant _currentTenant;

    public GetUnacknowledgedAlertsQueryHandler(IAuditAlertRepository repository, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<IReadOnlyList<AuditAlertDto>>> Handle(GetUnacknowledgedAlertsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId ?? string.Empty;
        var alerts = await _repository.GetUnacknowledgedAsync(tenantId, cancellationToken);

        var result = alerts.Adapt<List<AuditAlertDto>>();
        return Result.Success<IReadOnlyList<AuditAlertDto>>(result);
    }
}
