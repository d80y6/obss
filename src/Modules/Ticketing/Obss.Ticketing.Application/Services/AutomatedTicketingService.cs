using Microsoft.Extensions.Logging;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Domain.Entities;
using Obss.Ticketing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Ticketing.Application.Services;

public sealed class AutomatedTicketingService : IAutomatedTicketingService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AutomatedTicketingService> _logger;

    public AutomatedTicketingService(
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        ILogger<AutomatedTicketingService> logger)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TicketCreationResult> CreateTicketFromAlarmAsync(Alarm alarm, CancellationToken cancellationToken = default)
    {
        var priority = alarm.Severity switch
        {
            "CRITICAL" => TicketPriority.Critical,
            "MAJOR" => TicketPriority.High,
            "MINOR" => TicketPriority.Medium,
            _ => TicketPriority.Low
        };

        var category = TicketCategory.Technical;

        var assignedGroup = alarm.SourceType switch
        {
            "HUAWEI_OLT" => "Fiber Access Team",
            "ZTE_SOFTSWITCH" => "Voice Core Team",
            _ => "Network Operations"
        };

        var subject = $"[AUTO-{alarm.Severity}] {alarm.SpecificProblem ?? alarm.AlarmType} on {alarm.SourceName}";
        var description = $"Automatically created from alarm {alarm.AlarmId}.\n" +
                          $"Source: {alarm.SourceName} ({alarm.SourceType})\n" +
                          $"Severity: {alarm.Severity}\n" +
                          $"Problem: {alarm.SpecificProblem}\n" +
                          $"Raised: {alarm.RaisedTime:O}";

        var ticket = Ticket.Create(
            "system",
            await _ticketRepository.GetNextTicketNumberAsync(cancellationToken),
            Guid.TryParse(alarm.AffectedCustomerId, out var customerId) ? customerId : Guid.Empty,
            alarm.SourceName,
            subject,
            description,
            priority,
            category,
            TicketSource.API);

        if (alarm.AffectedCustomerId is not null)
        {
            ticket.Assign("system", assignedGroup);
        }

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Auto-created ticket {TicketNumber} (ID: {TicketId}) for alarm {AlarmId} with priority {Priority}",
            ticket.TicketNumber, ticket.Id, alarm.AlarmId, priority);

        return new TicketCreationResult(ticket.Id, ticket.TicketNumber, true);
    }

    public async Task UpdateTicketOnAlarmClearAsync(Alarm alarm, CancellationToken cancellationToken = default)
    {
        if (alarm.AffectedServiceId is null)
        {
            return;
        }

        var ticketNumber = $"ALM-{alarm.AlarmId}";
        var existing = await _ticketRepository.GetByTicketNumberAsync(ticketNumber, cancellationToken);

        if (existing is null)
        {
            _logger.LogWarning(
                "No ticket found for cleared alarm {AlarmId} (expected ticket: {TicketNumber})",
                alarm.AlarmId, ticketNumber);
            return;
        }

        existing.Resolve($"Alarm cleared at {alarm.ClearedTime:O}");
        await _ticketRepository.UpdateAsync(existing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Ticket {TicketNumber} resolved due to alarm {AlarmId} clearance",
            existing.TicketNumber, alarm.AlarmId);
    }
}
