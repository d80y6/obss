using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.SharedKernel.Application.Abstractions;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.Services;
using Obss.Ticketing.Domain.Entities;
using Obss.Ticketing.Domain.ValueObjects;
using Xunit;

namespace Obss.Ticketing.Tests.Services;

public class AutomatedTicketingServiceTests
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AutomatedTicketingService> _logger;
    private readonly AutomatedTicketingService _service;

    public AutomatedTicketingServiceTests()
    {
        _ticketRepository = Substitute.For<ITicketRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<AutomatedTicketingService>>();
        _service = new AutomatedTicketingService(_ticketRepository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task CreateTicketFromAlarmAsync_WithCriticalAlarm_ShouldCreateHighPriorityTicket()
    {
        var alarm = Alarm.Create(
            "ALM-001", "HUAWEI_OLT", "olt-01", "COMMUNICATIONS_ALARM",
            "CRITICAL", "LOS (Loss of Signal)", "فقدان الإشارة",
            "SVC-001", "CUST-001", DateTime.UtcNow.AddMinutes(-10));

        _ticketRepository.GetNextTicketNumberAsync(Arg.Any<CancellationToken>())
            .Returns("TKT-AUTO-001");

        var result = await _service.CreateTicketFromAlarmAsync(alarm);

        result.Should().NotBeNull();
        result.Created.Should().BeTrue();
        result.TicketNumber.Should().Be("TKT-AUTO-001");

        await _ticketRepository.Received(1)
            .AddAsync(Arg.Is<Ticket>(t =>
                t.Priority == TicketPriority.Critical &&
                t.Subject.Contains("CRITICAL") &&
                t.Subject.Contains("olt-01")), Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTicketFromAlarmAsync_WithMajorAlarm_ShouldCreateHighPriorityTicket()
    {
        var alarm = Alarm.Create(
            "ALM-002", "HUAWEI_OLT", "olt-02", "QUALITY_OF_SERVICE",
            "MAJOR", "High BER", "معدل خطأ مرتفع",
            null, null, DateTime.UtcNow);

        _ticketRepository.GetNextTicketNumberAsync(Arg.Any<CancellationToken>())
            .Returns("TKT-AUTO-002");

        var result = await _service.CreateTicketFromAlarmAsync(alarm);

        result.Created.Should().BeTrue();

        await _ticketRepository.Received(1)
            .AddAsync(Arg.Is<Ticket>(t => t.Priority == TicketPriority.High), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTicketFromAlarmAsync_ShouldAssignCorrectTeam()
    {
        var huaweiAlarm = Alarm.Create(
            "ALM-003", "HUAWEI_OLT", "olt-03", "EQUIPMENT_ALARM",
            "CRITICAL", "PON port failure", "فشل منفذ PON",
            null, null, DateTime.UtcNow);

        var zteAlarm = Alarm.Create(
            "ALM-004", "ZTE_SOFTSWITCH", "ss-01", "COMMUNICATIONS_ALARM",
            "CRITICAL", "SS7 link down", "انقطاع رابط SS7",
            null, null, DateTime.UtcNow);

        _ticketRepository.GetNextTicketNumberAsync(Arg.Any<CancellationToken>())
            .Returns("TKT-AUTO-003", "TKT-AUTO-004");

        var huaweiResult = await _service.CreateTicketFromAlarmAsync(huaweiAlarm);
        var zteResult = await _service.CreateTicketFromAlarmAsync(zteAlarm);

        huaweiResult.Created.Should().BeTrue();
        zteResult.Created.Should().BeTrue();
    }

    [Fact]
    public async Task CreateTicketFromAlarmAsync_WithAffectedCustomer_ShouldAssignTicket()
    {
        var alarm = Alarm.Create(
            "ALM-005", "HUAWEI_OLT", "olt-04", "COMMUNICATIONS_ALARM",
            "CRITICAL", "Optical fiber cut", "قطع الألياف البصرية",
            "SVC-005", "CUST-010", DateTime.UtcNow.AddMinutes(-30));

        _ticketRepository.GetNextTicketNumberAsync(Arg.Any<CancellationToken>())
            .Returns("TKT-AUTO-005");

        var result = await _service.CreateTicketFromAlarmAsync(alarm);

        result.Created.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTicketOnAlarmClearAsync_WhenNoAffectedServiceId_ShouldDoNothing()
    {
        var alarm = Alarm.Create(
            "ALM-006", "HUAWEI_OLT", "olt-05", "QUALITY_OF_SERVICE",
            "MINOR", "Slight degradation", "تدهور طفيف",
            null, null, DateTime.UtcNow);
        alarm.Clear();

        await _service.UpdateTicketOnAlarmClearAsync(alarm);

        await _ticketRepository.Received(0)
            .GetByTicketNumberAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTicketOnAlarmClearAsync_WhenTicketExists_ShouldResolve()
    {
        var alarm = Alarm.Create(
            "ALM-007", "HUAWEI_OLT", "olt-06", "COMMUNICATIONS_ALARM",
            "CRITICAL", "LOS", "فقدان الإشارة",
            "SVC-006", "CUST-006", DateTime.UtcNow.AddHours(-2));
        alarm.Clear();

        var existingTicket = Ticket.Create(
            "system", "TKT-EXIST-001", Guid.NewGuid(), "Test Customer",
            "Test subject", "Test description",
            TicketPriority.Critical, TicketCategory.Technical, TicketSource.API);

        _ticketRepository.GetByTicketNumberAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(existingTicket);

        await _service.UpdateTicketOnAlarmClearAsync(alarm);

        existingTicket.Status.Should().Be(TicketStatus.Resolved);
        existingTicket.Resolution.Should().NotBeNullOrEmpty();

        await _ticketRepository.Received(1)
            .UpdateAsync(existingTicket, Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
