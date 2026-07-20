using Obss.SharedKernel.Domain.Common;

namespace Obss.Ticketing.Domain.Entities;

public class Alarm : AggregateRoot<Guid>
{
    private Alarm() { }

    private Alarm(
        Guid id,
        string alarmId,
        string sourceType,
        string sourceName,
        string alarmType,
        string severity,
        string? specificProblem,
        string? specificProblemAr,
        string? affectedServiceId,
        string? affectedCustomerId,
        DateTime raisedTime)
        : base(id)
    {
        AlarmId = alarmId;
        SourceType = sourceType;
        SourceName = sourceName;
        AlarmType = alarmType;
        Severity = severity;
        SpecificProblem = specificProblem;
        SpecificProblemAr = specificProblemAr;
        AffectedServiceId = affectedServiceId;
        AffectedCustomerId = affectedCustomerId;
        RaisedTime = raisedTime;
        IsCleared = false;
        DuplicateCount = 1;
    }

    public string AlarmId { get; private set; } = string.Empty;
    public string SourceType { get; private set; } = string.Empty;
    public string SourceName { get; private set; } = string.Empty;
    public string AlarmType { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public string? SpecificProblem { get; private set; }
    public string? SpecificProblemAr { get; private set; }
    public string? AffectedServiceId { get; private set; }
    public string? AffectedCustomerId { get; private set; }
    public DateTime RaisedTime { get; private set; }
    public DateTime? AcknowledgedTime { get; private set; }
    public string? AcknowledgedBy { get; private set; }
    public DateTime? ClearedTime { get; private set; }
    public bool IsCleared { get; private set; }
    public int DuplicateCount { get; private set; }
    public string? CorrelationRule { get; private set; }
    public string? MaintenanceWindowId { get; private set; }

    public static Alarm Create(
        string alarmId,
        string sourceType,
        string sourceName,
        string alarmType,
        string severity,
        string? specificProblem,
        string? specificProblemAr,
        string? affectedServiceId,
        string? affectedCustomerId,
        DateTime raisedTime)
    {
        return new Alarm(
            Guid.NewGuid(),
            alarmId,
            sourceType,
            sourceName,
            alarmType,
            severity,
            specificProblem,
            specificProblemAr,
            affectedServiceId,
            affectedCustomerId,
            raisedTime);
    }

    public void Acknowledge(string acknowledgedBy)
    {
        AcknowledgedTime = DateTime.UtcNow;
        AcknowledgedBy = acknowledgedBy;
    }

    public void Clear()
    {
        ClearedTime = DateTime.UtcNow;
        IsCleared = true;
    }

    public void IncrementDuplicate()
    {
        DuplicateCount++;
    }

    public void AssignCorrelationRule(string rule)
    {
        CorrelationRule = rule;
    }

    public void AssignMaintenanceWindow(string maintenanceWindowId)
    {
        MaintenanceWindowId = maintenanceWindowId;
    }

    public bool IsSuppressedByMaintenance()
    {
        return MaintenanceWindowId is not null;
    }
}
