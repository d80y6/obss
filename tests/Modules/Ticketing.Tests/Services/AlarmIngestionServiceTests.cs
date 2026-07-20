using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.Services;
using Obss.Ticketing.Domain.Entities;
using Xunit;

namespace Obss.Ticketing.Tests.Services;

public class AlarmIngestionServiceTests
{
    private readonly IAlarmCorrelationService _correlationService;
    private readonly IAutomatedTicketingService _automatedTicketingService;
    private readonly ILogger<AlarmIngestionService> _logger;
    private readonly AlarmIngestionService _service;

    public AlarmIngestionServiceTests()
    {
        _correlationService = Substitute.For<IAlarmCorrelationService>();
        _automatedTicketingService = Substitute.For<IAutomatedTicketingService>();
        _logger = Substitute.For<ILogger<AlarmIngestionService>>();
        _service = new AlarmIngestionService(
            _correlationService,
            _automatedTicketingService,
            _logger);
    }

    [Fact]
    public async Task IngestAlarmAsync_WithCriticalAlarm_ShouldCreateTicket()
    {
        var alarm = Alarm.Create(
            "ALM-001", "HUAWEI_OLT", "olt-01", "COMMUNICATIONS_ALARM",
            "CRITICAL", "LOS (Loss of Signal)", "فقدان الإشارة",
            "SVC-001", "CUST-001", DateTime.UtcNow.AddMinutes(-10));

        _correlationService.IsSuppressedByMaintenanceAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _correlationService.CorrelateAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(new CorrelationResult(alarm.AlarmId, [alarm.Id], true, "ROOT_CAUSE_CRITICAL"));

        _automatedTicketingService.CreateTicketFromAlarmAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(new TicketCreationResult(Guid.NewGuid(), "TKT-001", true));

        var result = await _service.IngestAlarmAsync(alarm);

        result.Should().NotBeNull();
        result.IsDuplicate.Should().BeFalse();
        result.Suppressed.Should().BeFalse();
        result.TicketId.Should().NotBeNull();
        result.Error.Should().BeNull();

        await _automatedTicketingService.Received(1)
            .CreateTicketFromAlarmAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IngestAlarmAsync_WithSuppressedAlarm_ShouldNotCreateTicket()
    {
        var alarm = Alarm.Create(
            "ALM-002", "HUAWEI_OLT", "olt-01", "COMMUNICATIONS_ALARM",
            "MAJOR", "High BER", "معدل خطأ مرتفع",
            "SVC-002", "CUST-002", DateTime.UtcNow.AddMinutes(-5));

        _correlationService.IsSuppressedByMaintenanceAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _service.IngestAlarmAsync(alarm);

        result.Suppressed.Should().BeTrue();
        result.Error.Should().Be("Suppressed by maintenance window");

        await _automatedTicketingService.Received(0)
            .CreateTicketFromAlarmAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IngestAlarmAsync_WithMinorAlarm_ShouldNotCreateTicket()
    {
        var alarm = Alarm.Create(
            "ALM-003", "HUAWEI_OLT", "olt-01", "QUALITY_OF_SERVICE",
            "MINOR", "Slight packet loss", "فقدان بسيط للحزم",
            null, null, DateTime.UtcNow.AddMinutes(-2));

        _correlationService.IsSuppressedByMaintenanceAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _correlationService.CorrelateAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(new CorrelationResult(null, [], false, null));

        var result = await _service.IngestAlarmAsync(alarm);

        result.Should().NotBeNull();
        result.TicketId.Should().BeNull();

        await _automatedTicketingService.Received(0)
            .CreateTicketFromAlarmAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IngestAlarmAsync_ShouldNormalizeSeverity()
    {
        var alarm = Alarm.Create(
            "ALM-004", "HUAWEI_OLT", "olt-01", "EQUIPMENT_ALARM",
            "EMERGENCY", "Power supply failure", "فشل مزود الطاقة",
            "SVC-003", "CUST-003", DateTime.UtcNow);

        _correlationService.IsSuppressedByMaintenanceAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _correlationService.CorrelateAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(new CorrelationResult(alarm.AlarmId, [alarm.Id], true, "ROOT_CAUSE_CRITICAL"));

        _automatedTicketingService.CreateTicketFromAlarmAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(new TicketCreationResult(Guid.NewGuid(), "TKT-002", true));

        var result = await _service.IngestAlarmAsync(alarm);

        result.Should().NotBeNull();
        result.TicketId.Should().NotBeNull();

        await _automatedTicketingService.Received(1)
            .CreateTicketFromAlarmAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IngestAlarmAsync_ShouldCorrelateAlarm()
    {
        var alarm = Alarm.Create(
            "ALM-005", "HUAWEI_OLT", "olt-02", "COMMUNICATIONS_ALARM",
            "CRITICAL", "Optical fiber cut", "قطع الألياف البصرية",
            "SVC-004", "CUST-004", DateTime.UtcNow.AddMinutes(-20));

        _correlationService.IsSuppressedByMaintenanceAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _correlationService.CorrelateAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(new CorrelationResult(alarm.AlarmId, [alarm.Id], true, "ROOT_CAUSE_CRITICAL"));

        _automatedTicketingService.CreateTicketFromAlarmAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>())
            .Returns(new TicketCreationResult(Guid.NewGuid(), "TKT-003", true));

        var result = await _service.IngestAlarmAsync(alarm);

        result.Should().NotBeNull();
        result.TicketId.Should().NotBeNull();

        await _correlationService.Received(1)
            .CorrelateAsync(Arg.Any<Alarm>(), Arg.Any<CancellationToken>());
    }
}
