using Xunit;
using FluentAssertions;
using Obss.Reporting.Domain.Entities;
using Obss.Reporting.Domain.ValueObjects;
using Obss.Reporting.Infrastructure.Persistence.Repositories;

namespace Obss.Reporting.Tests;

public class RepositoryTests : ReportingIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveReportDefinition()
    {
        using var context = CreateDbContext();
        var repository = new ReportRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var reportDefinition = ReportDefinition.Create(
            tenantId,
            "Quarterly Summary",
            "Quarterly financial summary",
            ReportType.Financial,
            "PostgreSQL",
            "SELECT * FROM quarterly_data",
            OutputFormat.PDF,
            null);

        await repository.AddAsync(reportDefinition);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(reportDefinition.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Quarterly Summary");
        retrieved.Description.Should().Be("Quarterly financial summary");
        retrieved.ReportType.Should().Be(ReportType.Financial);
        retrieved.OutputFormat.Should().Be(OutputFormat.PDF);
        retrieved.DataSource.Should().Be("PostgreSQL");
        retrieved.Query.Should().Be("SELECT * FROM quarterly_data");
        retrieved.IsActive.Should().BeTrue();
        retrieved.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task CanActivateAndDeactivateReportDefinition()
    {
        using var context = CreateDbContext();
        var repository = new ReportRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var reportDefinition = ReportDefinition.Create(
            tenantId,
            "Toggled Report",
            null,
            ReportType.Operational,
            "MySQL",
            "SELECT 1",
            OutputFormat.CSV);

        await repository.AddAsync(reportDefinition);
        await context.SaveChangesAsync();

        reportDefinition.Deactivate();
        await context.SaveChangesAsync();

        var deactivated = await repository.GetByIdAsync(reportDefinition.Id);
        deactivated!.IsActive.Should().BeFalse();

        deactivated.Activate();
        await context.SaveChangesAsync();

        var reactivated = await repository.GetByIdAsync(reportDefinition.Id);
        reactivated!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanUpdateScheduleOnReportDefinition()
    {
        using var context = CreateDbContext();
        var repository = new ReportRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var reportDefinition = ReportDefinition.Create(
            tenantId,
            "Scheduled Report",
            null,
            ReportType.Service,
            "PostgreSQL",
            "SELECT * FROM services",
            OutputFormat.PDF);

        await repository.AddAsync(reportDefinition);
        await context.SaveChangesAsync();

        reportDefinition.SetSchedule("0 0 * * *");
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(reportDefinition.Id);
        retrieved!.Schedule.Should().Be("0 0 * * *");
    }

    [Fact]
    public async Task CanAddAndRetrieveReportExecution()
    {
        using var context = CreateDbContext();
        var repository = new ReportRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var reportDefinition = ReportDefinition.Create(
            tenantId,
            "Execution Test",
            null,
            ReportType.Custom,
            "PostgreSQL",
            "SELECT * FROM data",
            OutputFormat.HTML);

        await repository.AddAsync(reportDefinition);
        await context.SaveChangesAsync();

        var execution = new ReportExecution(
            Guid.NewGuid(),
            reportDefinition.Id,
            "user-1");

        await repository.AddExecutionAsync(execution);
        await context.SaveChangesAsync();

        var executions = await repository.GetExecutionsByReportDefinitionIdAsync(reportDefinition.Id);

        executions.Should().HaveCount(1);
        executions[0].Status.Should().Be(ExecutionStatus.Queued);
        executions[0].ExecutedBy.Should().Be("user-1");

        execution.Start();
        await context.SaveChangesAsync();

        var runningExecutions = await repository.GetExecutionsByReportDefinitionIdAsync(reportDefinition.Id);
        runningExecutions[0].Status.Should().Be(ExecutionStatus.Running);
        runningExecutions[0].StartedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CanCompleteAndFailReportExecution()
    {
        using var context = CreateDbContext();
        var repository = new ReportRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var reportDefinition = ReportDefinition.Create(
            tenantId,
            "Complete Fail Test",
            null,
            ReportType.Usage,
            "PostgreSQL",
            "SELECT * FROM usage",
            OutputFormat.Excel);

        await repository.AddAsync(reportDefinition);
        await context.SaveChangesAsync();

        var execution = new ReportExecution(Guid.NewGuid(), reportDefinition.Id, "tester");
        await repository.AddExecutionAsync(execution);
        await context.SaveChangesAsync();

        execution.Start();
        execution.Complete("/reports/output.xlsx", 1024);
        await context.SaveChangesAsync();

        var completed = await repository.GetExecutionsByReportDefinitionIdAsync(reportDefinition.Id);
        completed[0].Status.Should().Be(ExecutionStatus.Completed);
        completed[0].FilePath.Should().Be("/reports/output.xlsx");
        completed[0].FileSize.Should().Be(1024);

        var failedExecution = new ReportExecution(Guid.NewGuid(), reportDefinition.Id, "tester");
        await repository.AddExecutionAsync(failedExecution);
        await context.SaveChangesAsync();

        failedExecution.Fail("Insufficient permissions");
        await context.SaveChangesAsync();

        var allExecutions = await repository.GetExecutionsByReportDefinitionIdAsync(reportDefinition.Id);
        allExecutions.Should().HaveCount(2);
        allExecutions.Should().Contain(e => e.Status == ExecutionStatus.Failed && e.ErrorMessage == "Insufficient permissions");
    }

    [Fact]
    public async Task CanAddAndRetrieveScheduledReport()
    {
        using var context = CreateDbContext();
        var repository = new ReportRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var reportDefinition = ReportDefinition.Create(
            tenantId,
            "Scheduled Source",
            null,
            ReportType.Network,
            "ClickHouse",
            "SELECT * FROM metrics",
            OutputFormat.CSV);

        await repository.AddAsync(reportDefinition);
        await context.SaveChangesAsync();

        var scheduledReport = ScheduledReport.Create(
            tenantId,
            reportDefinition.Id,
            "0 6 * * 1-5",
            ["ops@example.com"]);

        await repository.AddScheduledReportAsync(scheduledReport);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetScheduledReportByIdAsync(scheduledReport.Id);

        retrieved.Should().NotBeNull();
        retrieved!.TenantId.Should().Be(tenantId);
        retrieved.ReportDefinitionId.Should().Be(reportDefinition.Id);
        retrieved.CronExpression.Should().Be("0 6 * * 1-5");
        retrieved.Recipients.Should().BeEquivalentTo("ops@example.com");
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanQueryScheduledReportsByTenant()
    {
        using var context = CreateDbContext();
        var repository = new ReportRepository(context);

        var tenantId1 = Guid.NewGuid().ToString("N");
        var tenantId2 = Guid.NewGuid().ToString("N");

        var def1 = ReportDefinition.Create(tenantId1, "T1 Report", null, ReportType.Financial, "PG", "SELECT 1", OutputFormat.PDF);
        var def2 = ReportDefinition.Create(tenantId2, "T2 Report", null, ReportType.Financial, "PG", "SELECT 1", OutputFormat.PDF);

        await repository.AddAsync(def1);
        await repository.AddAsync(def2);
        await context.SaveChangesAsync();

        var sched1 = ScheduledReport.Create(tenantId1, def1.Id, "0 0 * * *", ["a@a.com"]);
        var sched2 = ScheduledReport.Create(tenantId1, def1.Id, "0 12 * * *", ["b@b.com"]);
        var sched3 = ScheduledReport.Create(tenantId2, def2.Id, "0 0 * * *", ["c@c.com"]);

        await repository.AddScheduledReportAsync(sched1);
        await repository.AddScheduledReportAsync(sched2);
        await repository.AddScheduledReportAsync(sched3);
        await context.SaveChangesAsync();

        var tenant1Schedules = await repository.GetScheduledReportsByTenantAsync(tenantId1);

        tenant1Schedules.Should().HaveCount(2);
        tenant1Schedules.Should().Contain(s => s.CronExpression == "0 0 * * *");
        tenant1Schedules.Should().Contain(s => s.CronExpression == "0 12 * * *");

        sched1.Deactivate();
        await context.SaveChangesAsync();

        var activeSchedules = await repository.GetScheduledReportsByTenantAsync(tenantId1);
        activeSchedules.Should().HaveCount(1);
    }

    [Fact]
    public async Task CanQueryScheduledReportsDue()
    {
        using var context = CreateDbContext();
        var repository = new ReportRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var def = ReportDefinition.Create(tenantId, "Due Test", null, ReportType.Operational, "PG", "SELECT 1", OutputFormat.PDF);
        await repository.AddAsync(def);
        await context.SaveChangesAsync();

        var due = ScheduledReport.Create(tenantId, def.Id, "0 0 * * *", ["due@example.com"]);
        due.SetNextRun(DateTime.UtcNow.AddHours(-1));

        var future = ScheduledReport.Create(tenantId, def.Id, "0 0 * * *", ["future@example.com"]);
        future.SetNextRun(DateTime.UtcNow.AddHours(2));

        var inactive = ScheduledReport.Create(tenantId, def.Id, "0 0 * * *", ["inactive@example.com"]);
        inactive.SetNextRun(DateTime.UtcNow.AddHours(-1));
        inactive.Deactivate();

        await repository.AddScheduledReportAsync(due);
        await repository.AddScheduledReportAsync(future);
        await repository.AddScheduledReportAsync(inactive);
        await context.SaveChangesAsync();

        var dueSchedules = await repository.GetScheduledReportsDueAsync(DateTime.UtcNow);

        dueSchedules.Should().Contain(s => s.Recipients.Contains("due@example.com"));
        dueSchedules.Should().NotContain(s => s.Recipients.Contains("future@example.com"));
        dueSchedules.Should().NotContain(s => s.Recipients.Contains("inactive@example.com"));
    }

    [Fact]
    public async Task CanAddAndRetrieveDashboardWidget()
    {
        using var context = CreateDbContext();
        var repository = new ReportRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var widget = DashboardWidget.Create(
            tenantId,
            WidgetType.Chart,
            "Revenue Chart",
            "{\"type\": \"bar\"}",
            1,
            WidgetSize.Medium,
            "PostgreSQL",
            "SELECT * FROM revenue",
            300);

        await repository.AddWidgetAsync(widget);
        await context.SaveChangesAsync();

        var widgets = await repository.GetWidgetsByTenantAsync(tenantId);

        widgets.Should().HaveCount(1);
        widgets[0].Title.Should().Be("Revenue Chart");
        widgets[0].WidgetType.Should().Be(WidgetType.Chart);
        widgets[0].Position.Should().Be(1);
        widgets[0].Size.Should().Be(WidgetSize.Medium);
        widgets[0].RefreshInterval.Should().Be(300);
        widgets[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanAddMultipleWidgetsInOrder()
    {
        using var context = CreateDbContext();
        var repository = new ReportRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");

        var widget1 = DashboardWidget.Create(tenantId, WidgetType.Metric, "Metric 1", "{}", 1, WidgetSize.Small, "PG", "SELECT 1");
        var widget2 = DashboardWidget.Create(tenantId, WidgetType.Table, "Table 1", "{}", 2, WidgetSize.Large, "PG", "SELECT 2");
        var widget3 = DashboardWidget.Create(tenantId, WidgetType.KPI, "KPI 1", "{}", 3, WidgetSize.Medium, "PG", "SELECT 3");

        await repository.AddWidgetAsync(widget1);
        await repository.AddWidgetAsync(widget2);
        await repository.AddWidgetAsync(widget3);
        await context.SaveChangesAsync();

        var widgets = await repository.GetWidgetsByTenantAsync(tenantId);

        widgets.Should().HaveCount(3);
        widgets.Should().BeInAscendingOrder(w => w.Position);
        widgets[0].Title.Should().Be("Metric 1");
        widgets[1].Title.Should().Be("Table 1");
        widgets[2].Title.Should().Be("KPI 1");

        widget2.Deactivate();
        await context.SaveChangesAsync();

        var activeWidgets = await repository.GetWidgetsByTenantAsync(tenantId);
        activeWidgets.Should().HaveCount(2);
        activeWidgets.Should().NotContain(w => w.Title == "Table 1");
    }
}
