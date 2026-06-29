using Mapster;
using MediatR;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Reporting.Application.Queries.GetScheduledReports;

public sealed class GetScheduledReportsQueryHandler : IRequestHandler<GetScheduledReportsQuery, Result<IReadOnlyList<ScheduledReportDto>>>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICurrentTenant _currentTenant;

    public GetScheduledReportsQueryHandler(IReportRepository reportRepository, ICurrentTenant currentTenant)
    {
        _reportRepository = reportRepository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<IReadOnlyList<ScheduledReportDto>>> Handle(GetScheduledReportsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId ?? _currentTenant.TenantId;
        if (string.IsNullOrEmpty(tenantId))
            return Result.Failure<IReadOnlyList<ScheduledReportDto>>(new Error("TENANT_REQUIRED", "TenantId is required"));

        var scheduledReports = await _reportRepository.GetScheduledReportsByTenantAsync(tenantId, cancellationToken);
        return Result.Success(scheduledReports.Adapt<IReadOnlyList<ScheduledReportDto>>());
    }
}
