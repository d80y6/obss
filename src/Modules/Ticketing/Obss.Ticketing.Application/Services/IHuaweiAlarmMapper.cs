using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Services;

public sealed record HuaweiSnmpTrap(
    string AlarmId,
    string SourceName,
    string AlarmType,
    int SeverityCode,
    string? SpecificProblem,
    string? SpecificProblemAr,
    string? AffectedServiceId,
    string? AffectedCustomerId,
    DateTime RaisedTime);

public sealed record HuaweiRestconfAlarm(
    string AlarmId,
    string SourceName,
    string AlarmType,
    string Severity,
    string? SpecificProblem,
    string? SpecificProblemAr,
    string? AffectedServiceId,
    string? AffectedCustomerId,
    DateTime RaisedTime);

public interface IHuaweiAlarmMapper
{
    Alarm MapFromSnmpTrap(HuaweiSnmpTrap trap);
    Alarm MapFromRestconf(HuaweiRestconfAlarm alarm);
}
