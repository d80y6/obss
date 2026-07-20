using Microsoft.Extensions.Logging;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Services;

public sealed class AlarmIngestionService : IAlarmIngestionService
{
    private readonly IAlarmCorrelationService _correlationService;
    private readonly IAutomatedTicketingService _automatedTicketingService;
    private readonly ILogger<AlarmIngestionService> _logger;
    private static readonly HashSet<string> KnownSeverities =
    [
        "CRITICAL", "MAJOR", "MINOR", "WARNING", "INFO"
    ];

    public AlarmIngestionService(
        IAlarmCorrelationService correlationService,
        IAutomatedTicketingService automatedTicketingService,
        ILogger<AlarmIngestionService> logger)
    {
        _correlationService = correlationService;
        _automatedTicketingService = automatedTicketingService;
        _logger = logger;
    }

    public async Task<AlarmIngestionResult> IngestAlarmAsync(Alarm alarm, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeSeverity(alarm);

        if (normalized is not null)
        {
            alarm = normalized;
        }

        var suppressed = await _correlationService.IsSuppressedByMaintenanceAsync(alarm, cancellationToken);

        if (suppressed)
        {
            _logger.LogInformation(
                "Alarm {AlarmId} from {SourceName} suppressed by maintenance window",
                alarm.AlarmId, alarm.SourceName);

            return new AlarmIngestionResult(alarm.Id, false, true, null, "Suppressed by maintenance window");
        }

        var correlation = await _correlationService.CorrelateAsync(alarm, cancellationToken);

        if (correlation.CorrelationRule is not null)
        {
            alarm.AssignCorrelationRule(correlation.CorrelationRule);
        }

        Guid? ticketId = null;

        if (alarm.Severity == "CRITICAL" || alarm.Severity == "MAJOR")
        {
            var ticketResult = await _automatedTicketingService.CreateTicketFromAlarmAsync(alarm, cancellationToken);
            ticketId = ticketResult.TicketId;

            _logger.LogInformation(
                "Ticket {TicketNumber} (ID: {TicketId}) created for alarm {AlarmId}",
                ticketResult.TicketNumber, ticketResult.TicketId, alarm.AlarmId);
        }

        if (!suppressed && alarm.Severity == "CRITICAL" && !alarm.IsCleared)
        {
            await _correlationService.EscalateIfNeededAsync(alarm, cancellationToken);
        }

        _logger.LogInformation(
            "Alarm {AlarmId} ingested. Severity: {Severity}, Correlated: {Correlated}, Ticket: {TicketId}",
            alarm.AlarmId, alarm.Severity, correlation.IsRootCause, ticketId);

        return new AlarmIngestionResult(alarm.Id, false, false, ticketId, null);
    }

    private static Alarm? NormalizeSeverity(Alarm alarm)
    {
        var normalized = alarm.Severity.ToUpperInvariant();

        if (KnownSeverities.Contains(normalized))
        {
            return null;
        }

        if (normalized is "EMERGENCY" or "FATAL")
        {
            normalized = "CRITICAL";
        }
        else if (normalized is "ERROR" or "FAILURE")
        {
            normalized = "MAJOR";
        }
        else if (normalized is "WARN" or "NOTICE")
        {
            normalized = "WARNING";
        }
        else if (normalized is "DEBUG" or "TRACE")
        {
            normalized = "INFO";
        }
        else
        {
            normalized = "MINOR";
        }

        return Alarm.Create(
            alarm.AlarmId,
            alarm.SourceType,
            alarm.SourceName,
            alarm.AlarmType,
            normalized,
            alarm.SpecificProblem,
            alarm.SpecificProblemAr,
            alarm.AffectedServiceId,
            alarm.AffectedCustomerId,
            alarm.RaisedTime);
    }
}
