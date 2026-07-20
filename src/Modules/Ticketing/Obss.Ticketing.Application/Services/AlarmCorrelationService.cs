using Microsoft.Extensions.Logging;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Services;

public sealed class AlarmCorrelationService : IAlarmCorrelationService
{
    private readonly IAlarmIngestionService _alarmIngestionService;
    private readonly ILogger<AlarmCorrelationService> _logger;
    private static readonly TimeSpan EscalationThreshold = TimeSpan.FromMinutes(15);

    public AlarmCorrelationService(
        IAlarmIngestionService alarmIngestionService,
        ILogger<AlarmCorrelationService> logger)
    {
        _alarmIngestionService = alarmIngestionService;
        _logger = logger;
    }

    public Task<CorrelationResult> CorrelateAsync(Alarm alarm, CancellationToken cancellationToken = default)
    {
        var correlatedIds = new List<Guid>();
        string? rootCauseId = null;
        string? rule = null;

        if (alarm.Severity == "CRITICAL")
        {
            rootCauseId = alarm.AlarmId;
            correlatedIds.Add(alarm.Id);
            rule = "ROOT_CAUSE_CRITICAL";

            _logger.LogInformation(
                "Alarm {AlarmId} identified as root cause (critical severity, rule: {Rule})",
                alarm.AlarmId, rule);
        }

        var result = new CorrelationResult(rootCauseId, correlatedIds.AsReadOnly(), rootCauseId is not null, rule);
        return Task.FromResult(result);
    }

    public Task<bool> IsSuppressedByMaintenanceAsync(Alarm alarm, CancellationToken cancellationToken = default)
    {
        var suppressed = alarm.IsSuppressedByMaintenance();

        if (suppressed)
        {
            _logger.LogInformation(
                "Alarm {AlarmId} suppressed by maintenance window {WindowId}",
                alarm.AlarmId, alarm.MaintenanceWindowId);
        }

        return Task.FromResult(suppressed);
    }

    public async Task EscalateIfNeededAsync(Alarm alarm, CancellationToken cancellationToken = default)
    {
        if (alarm.Severity != "CRITICAL")
        {
            return;
        }

        if (alarm.IsCleared)
        {
            return;
        }

        if (alarm.AcknowledgedTime.HasValue)
        {
            return;
        }

        var elapsed = DateTime.UtcNow - alarm.RaisedTime;

        if (elapsed > EscalationThreshold)
        {
            _logger.LogWarning(
                "Critical alarm {AlarmId} from {SourceName} unacknowledged for {Elapsed:F0} min. Escalating.",
                alarm.AlarmId, alarm.SourceName, elapsed.TotalMinutes);

            var ingested = await _alarmIngestionService.IngestAlarmAsync(alarm, cancellationToken);

            if (ingested.TicketId.HasValue)
            {
                _logger.LogInformation(
                    "Escalation ticket {TicketId} created for unacknowledged critical alarm {AlarmId}",
                    ingested.TicketId, alarm.AlarmId);
            }
        }
    }
}
