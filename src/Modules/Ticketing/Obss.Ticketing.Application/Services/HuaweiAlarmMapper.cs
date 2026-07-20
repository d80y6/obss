using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Services;

public sealed class HuaweiAlarmMapper : IHuaweiAlarmMapper
{
    public Alarm MapFromSnmpTrap(HuaweiSnmpTrap trap)
    {
        var severity = trap.SeverityCode switch
        {
            1 => "CRITICAL",
            2 => "MAJOR",
            3 => "MINOR",
            4 => "WARNING",
            _ => "INFO"
        };

        return Alarm.Create(
            trap.AlarmId,
            "HUAWEI_OLT",
            trap.SourceName,
            trap.AlarmType,
            severity,
            trap.SpecificProblem,
            trap.SpecificProblemAr,
            trap.AffectedServiceId,
            trap.AffectedCustomerId,
            trap.RaisedTime);
    }

    public Alarm MapFromRestconf(HuaweiRestconfAlarm alarm)
    {
        return Alarm.Create(
            alarm.AlarmId,
            "HUAWEI_OLT",
            alarm.SourceName,
            alarm.AlarmType,
            alarm.Severity,
            alarm.SpecificProblem,
            alarm.SpecificProblemAr,
            alarm.AffectedServiceId,
            alarm.AffectedCustomerId,
            alarm.RaisedTime);
    }
}
