using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Services;

// NOTE: ZTE alarm format not vendor-confirmed.
// This is a boundary interface based on documented ZTE softswitch alarm notification structure.
// Actual field names and nesting may differ from production ZTE NMS.
// Blocked: pending vendor confirmation of alarm notification schema.
public sealed record ZteAlarmNotification(
    string AlarmId,
    string SourceName,
    int SeverityCode,
    string? SpecificProblem,
    string? AffectedServiceId,
    string? AffectedCustomerId,
    DateTime RaisedTime);

public interface IZteAlarmMapper
{
    Alarm MapFromNotification(ZteAlarmNotification notification);
}
