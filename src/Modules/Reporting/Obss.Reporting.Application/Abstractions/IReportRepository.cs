using Obss.Reporting.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Reporting.Application.Abstractions;

public interface IReportRepository : IRepository<ReportDefinition>
{
    Task AddExecutionAsync(ReportExecution execution, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReportExecution>> GetExecutionsByReportDefinitionIdAsync(Guid reportDefinitionId, CancellationToken cancellationToken = default);
    Task AddWidgetAsync(DashboardWidget widget, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DashboardWidget>> GetWidgetsByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task AddScheduledReportAsync(ScheduledReport scheduledReport, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScheduledReport>> GetScheduledReportsByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScheduledReport>> GetScheduledReportsDueAsync(DateTime now, CancellationToken cancellationToken = default);
    Task<ScheduledReport?> GetScheduledReportByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
