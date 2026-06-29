using Microsoft.EntityFrameworkCore;
using Obss.Reporting.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Reporting.Infrastructure.Persistence;

public class ReportDbContext : EfDbContext
{
    public ReportDbContext(
        DbContextOptions<ReportDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<ReportExecution> ReportExecutions => Set<ReportExecution>();
    public DbSet<DashboardWidget> DashboardWidgets => Set<DashboardWidget>();
    public DbSet<ScheduledReport> ScheduledReports => Set<ScheduledReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
