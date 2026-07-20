using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Services;

public sealed class ZteAlarmMapper : IZteAlarmMapper
{
    public Alarm MapFromNotification(ZteAlarmNotification notification)
    {
        var severity = notification.SeverityCode switch
        {
            1 => "CRITICAL",
            2 => "MAJOR",
            3 => "MINOR",
            4 => "WARNING",
            _ => "INFO"
        };

        return Alarm.Create(
            notification.AlarmId,
            "ZTE_SOFTSWITCH",
            notification.SourceName,
            "COMMUNICATIONS_ALARM",
            severity,
            notification.SpecificProblem,
            null,
            notification.AffectedServiceId,
            notification.AffectedCustomerId,
            notification.RaisedTime);
    }
}
