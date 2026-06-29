using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Reporting.Application.Commands.CreateReportDefinition;
using Obss.Reporting.Application.Commands.ScheduleReport;
using Obss.Reporting.Infrastructure.Persistence;
using Obss.Reporting.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Reporting.Tests;

public class CommandHandlerTests : ReportingIntegrationTests
{
    [Fact]
    public async Task CreateReportDefinitionCommand_ShouldCreateReportDefinitionInDatabase()
    {
        using var context = CreateDbContext();
        var reportRepository = new ReportRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateReportDefinitionCommandHandler(reportRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateReportDefinitionCommand(
            tenantId,
            "Monthly Revenue",
            "Monthly revenue report",
            "Financial",
            "PostgreSQL",
            "SELECT * FROM revenue WHERE date >= @start AND date <= @end",
            "PDF",
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Monthly Revenue");
        result.Value.Description.Should().Be("Monthly revenue report");
        result.Value.ReportType.Should().Be("Financial");
        result.Value.OutputFormat.Should().Be("PDF");
        result.Value.DataSource.Should().Be("PostgreSQL");
        result.Value.Query.Should().Be("SELECT * FROM revenue WHERE date >= @start AND date <= @end");
        result.Value.IsActive.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);

        var saved = await reportRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Monthly Revenue");
    }

    [Fact]
    public async Task CreateReportDefinitionCommand_ShouldFailWithInvalidReportType()
    {
        using var context = CreateDbContext();
        var reportRepository = new ReportRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateReportDefinitionCommandHandler(reportRepository, unitOfWork);

        var command = new CreateReportDefinitionCommand(
            Guid.NewGuid().ToString("N"),
            "Invalid Report",
            null,
            "InvalidType",
            "PostgreSQL",
            "SELECT 1",
            "PDF",
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Description.Should().Contain("Invalid report type");
    }

    [Fact]
    public async Task CreateReportDefinitionCommand_ShouldFailWithInvalidOutputFormat()
    {
        using var context = CreateDbContext();
        var reportRepository = new ReportRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateReportDefinitionCommandHandler(reportRepository, unitOfWork);

        var command = new CreateReportDefinitionCommand(
            Guid.NewGuid().ToString("N"),
            "Invalid Format",
            null,
            "Financial",
            "PostgreSQL",
            "SELECT 1",
            "InvalidFormat",
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Description.Should().Contain("Invalid output format");
    }

    [Fact]
    public async Task CreateReportDefinitionCommand_ShouldCreateReportWithVariousTypes()
    {
        using var context = CreateDbContext();
        var reportRepository = new ReportRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateReportDefinitionCommandHandler(reportRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateReportDefinitionCommand(
            tenantId,
            "Network Usage",
            "Network usage analytics",
            "Network",
            "ClickHouse",
            "SELECT * FROM network_events",
            "CSV",
            "0 0 * * *");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReportType.Should().Be("Network");
        result.Value.OutputFormat.Should().Be("CSV");
        result.Value.Schedule.Should().Be("0 0 * * *");
    }

    [Fact]
    public async Task ScheduleReportCommand_ShouldCreateScheduledReport()
    {
        using var context = CreateDbContext();
        var reportRepository = new ReportRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateReportDefinitionCommandHandler(reportRepository, unitOfWork);
        var scheduleHandler = new ScheduleReportCommandHandler(reportRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var createCommand = new CreateReportDefinitionCommand(
            tenantId,
            "Scheduled Report",
            null,
            "Operational",
            "PostgreSQL",
            "SELECT * FROM operations",
            "PDF",
            null);

        var reportResult = await createHandler.Handle(createCommand, CancellationToken.None);

        var scheduleCommand = new ScheduleReportCommand(
            tenantId,
            reportResult.Value.Id,
            "0 8 * * 1",
            ["admin@example.com", "manager@example.com"]);

        var result = await scheduleHandler.Handle(scheduleCommand, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ReportDefinitionId.Should().Be(reportResult.Value.Id);
        result.Value.CronExpression.Should().Be("0 8 * * 1");
        result.Value.Recipients.Should().BeEquivalentTo("admin@example.com", "manager@example.com");
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.IsActive.Should().BeTrue();

        var savedSchedule = await reportRepository.GetScheduledReportByIdAsync(result.Value.Id);
        savedSchedule.Should().NotBeNull();
        savedSchedule!.CronExpression.Should().Be("0 8 * * 1");
    }

    [Fact]
    public async Task ScheduleReportCommand_ShouldFailWhenReportDefinitionNotFound()
    {
        using var context = CreateDbContext();
        var reportRepository = new ReportRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new ScheduleReportCommandHandler(reportRepository, unitOfWork);

        var command = new ScheduleReportCommand(
            Guid.NewGuid().ToString("N"),
            Guid.NewGuid(),
            "0 0 * * *",
            ["test@example.com"]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
