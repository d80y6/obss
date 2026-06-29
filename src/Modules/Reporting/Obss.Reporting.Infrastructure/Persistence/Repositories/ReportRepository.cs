using Microsoft.EntityFrameworkCore;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Reporting.Infrastructure.Persistence.Repositories;

public sealed class ReportRepository : EfRepository<ReportDefinition>, IReportRepository
{
    public ReportRepository(ReportDbContext context)
        : base(context)
    {
    }

    public async Task AddExecutionAsync(ReportExecution execution, CancellationToken cancellationToken = default)
    {
        await Context.Set<ReportExecution>().AddAsync(execution, cancellationToken);
    }

    public async Task<IReadOnlyList<ReportExecution>> GetExecutionsByReportDefinitionIdAsync(Guid reportDefinitionId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ReportExecution>()
            .Where(e => e.ReportDefinitionId == reportDefinitionId)
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddWidgetAsync(DashboardWidget widget, CancellationToken cancellationToken = default)
    {
        await Context.Set<DashboardWidget>().AddAsync(widget, cancellationToken);
    }

    public async Task<IReadOnlyList<DashboardWidget>> GetWidgetsByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<DashboardWidget>()
            .Where(w => w.TenantId == tenantId && w.IsActive)
            .OrderBy(w => w.Position)
            .ToListAsync(cancellationToken);
    }

    public async Task AddScheduledReportAsync(ScheduledReport scheduledReport, CancellationToken cancellationToken = default)
    {
        await Context.Set<ScheduledReport>().AddAsync(scheduledReport, cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduledReport>> GetScheduledReportsByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ScheduledReport>()
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduledReport>> GetScheduledReportsDueAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ScheduledReport>()
            .Where(s => s.IsActive && s.NextRunAt <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<ScheduledReport?> GetScheduledReportByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ScheduledReport>().FindAsync([id], cancellationToken);
    }
}
