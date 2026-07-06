using Mapster;
using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetAlerts;

public sealed class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, Result<IReadOnlyList<AuditAlertDto>>>
{
    private readonly IAuditAlertRepository _repository;
    private readonly ICurrentTenant _currentTenant;

    public GetAlertsQueryHandler(IAuditAlertRepository repository, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<IReadOnlyList<AuditAlertDto>>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
    {
        var alerts = await _repository.GetFilteredAsync(
            _currentTenant.TenantId,
            request.Severity,
            request.AlertType,
            request.IsAcknowledged,
            request.FromDate,
            request.ToDate,
            request.Offset,
            request.Limit,
            cancellationToken);

        var result = alerts.Adapt<List<AuditAlertDto>>();
        return Result.Success<IReadOnlyList<AuditAlertDto>>(result);
    }
}
