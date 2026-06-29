using Mapster;
using MediatR;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetDashboardConfig;

public sealed class GetDashboardConfigQueryHandler : IRequestHandler<GetDashboardConfigQuery, Result<IReadOnlyList<DashboardWidgetDto>>>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICurrentTenant _currentTenant;

    public GetDashboardConfigQueryHandler(IReportRepository reportRepository, ICurrentTenant currentTenant)
    {
        _reportRepository = reportRepository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<IReadOnlyList<DashboardWidgetDto>>> Handle(GetDashboardConfigQuery request, CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId ?? _currentTenant.TenantId;
        if (string.IsNullOrEmpty(tenantId))
            return Result.Success<IReadOnlyList<DashboardWidgetDto>>([]);

        var widgets = await _reportRepository.GetWidgetsByTenantAsync(tenantId, cancellationToken);
        return Result.Success(widgets.Adapt<IReadOnlyList<DashboardWidgetDto>>());
    }
}
