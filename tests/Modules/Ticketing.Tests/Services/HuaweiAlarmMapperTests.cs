using FluentAssertions;
using Obss.Ticketing.Application.Services;
using Obss.Ticketing.Domain.Entities;
using Xunit;

namespace Obss.Ticketing.Tests.Services;

public class HuaweiAlarmMapperTests
{
    private readonly HuaweiAlarmMapper _mapper;

    public HuaweiAlarmMapperTests()
    {
        _mapper = new HuaweiAlarmMapper();
    }

    [Fact]
    public void MapFromSnmpTrap_WithCriticalSeverity_ShouldMapCorrectly()
    {
        var trap = new HuaweiSnmpTrap(
            "ALM-HW-001",
            "OLT-SITE-A",
            "COMMUNICATIONS_ALARM",
            1,
            "Loss of Signal",
            "فقدان الإشارة",
            "SVC-FTTH-001",
            "CUST-001",
            DateTime.UtcNow);

        var alarm = _mapper.MapFromSnmpTrap(trap);

        alarm.Should().NotBeNull();
        alarm.AlarmId.Should().Be("ALM-HW-001");
        alarm.SourceType.Should().Be("HUAWEI_OLT");
        alarm.SourceName.Should().Be("OLT-SITE-A");
        alarm.AlarmType.Should().Be("COMMUNICATIONS_ALARM");
        alarm.Severity.Should().Be("CRITICAL");
        alarm.SpecificProblem.Should().Be("Loss of Signal");
        alarm.SpecificProblemAr.Should().Be("فقدان الإشارة");
        alarm.AffectedServiceId.Should().Be("SVC-FTTH-001");
        alarm.AffectedCustomerId.Should().Be("CUST-001");
        alarm.IsCleared.Should().BeFalse();
        alarm.DuplicateCount.Should().Be(1);
    }

    [Fact]
    public void MapFromSnmpTrap_WithWarningSeverity_ShouldMapCorrectly()
    {
        var trap = new HuaweiSnmpTrap(
            "ALM-HW-002",
            "OLT-SITE-B",
            "QUALITY_OF_SERVICE",
            4,
            "High latency detected",
            "اكتشاف زمن وصول مرتفع",
            null,
            null,
            DateTime.UtcNow);

        var alarm = _mapper.MapFromSnmpTrap(trap);

        alarm.Severity.Should().Be("WARNING");
        alarm.AffectedServiceId.Should().BeNull();
        alarm.AffectedCustomerId.Should().BeNull();
    }

    [Fact]
    public void MapFromSnmpTrap_WithUnknownSeverityCode_ShouldDefaultToInfo()
    {
        var trap = new HuaweiSnmpTrap(
            "ALM-HW-003",
            "OLT-SITE-C",
            "ENVIRONMENTAL",
            99,
            "Unknown code",
            "رمز غير معروف",
            null,
            null,
            DateTime.UtcNow);

        var alarm = _mapper.MapFromSnmpTrap(trap);

        alarm.Severity.Should().Be("INFO");
    }

    [Fact]
    public void MapFromRestconf_ShouldMapCorrectly()
    {
        var restconf = new HuaweiRestconfAlarm(
            "ALM-HW-010",
            "OLT-SITE-D",
            "EQUIPMENT_ALARM",
            "MAJOR",
            "Fan failure",
            "فشل المروحة",
            "SVC-FTTH-002",
            "CUST-005",
            DateTime.UtcNow.AddHours(-1));

        var alarm = _mapper.MapFromRestconf(restconf);

        alarm.Should().NotBeNull();
        alarm.AlarmId.Should().Be("ALM-HW-010");
        alarm.SourceType.Should().Be("HUAWEI_OLT");
        alarm.Severity.Should().Be("MAJOR");
        alarm.SpecificProblem.Should().Be("Fan failure");
        alarm.SpecificProblemAr.Should().Be("فشل المروحة");
        alarm.AffectedServiceId.Should().Be("SVC-FTTH-002");
        alarm.RaisedTime.Should().BeCloseTo(DateTime.UtcNow.AddHours(-1), TimeSpan.FromSeconds(5));
    }
}
