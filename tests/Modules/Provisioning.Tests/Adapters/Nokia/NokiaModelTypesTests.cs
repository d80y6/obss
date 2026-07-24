using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Nokia;

public class NokiaModelTypesTests
{
    [Fact]
    public void InterfaceConfig_CanBeConstructed()
    {
        var sut = new InterfaceConfig("1/1/1", "Uplink", true, 1500, "100", "10.0.0.1", 24);
        sut.PortId.Should().Be("1/1/1");
    }

    [Fact]
    public void BgpConfig_CanBeConstructed()
    {
        var sut = new BgpConfig(65001, "10.0.0.1", Array.Empty<BgpGroup>());
        sut.AsNumber.Should().Be(65001);
    }

    [Fact]
    public void IpFilterConfig_CanBeConstructed()
    {
        var entries = new[] { new IpFilterEntry(10, "accept", "10.0.0.0/8", null, null, null, null, false) };
        var sut = new IpFilterConfig("filter-1", "Management access", entries);
        sut.Name.Should().Be("filter-1");
    }

    [Fact]
    public void DeviceInventory_CanBeConstructed()
    {
        var comps = new[] { new ChassisComponent("1", "port", "SN001", "750-12345", "10G SFP+", "Rev1") };
        var sut = new DeviceInventory("7750 SR-12", "SN123", "3HE 12345", "Nokia 7750 SR", "20.10.R1", comps);
        sut.Model.Should().Be("7750 SR-12");
    }

    [Fact]
    public void AlarmInfo_CanBeConstructed()
    {
        var sut = new AlarmInfo("1", "critical", "Port down", "1/1/1", DateTime.UtcNow);
        sut.Severity.Should().Be("critical");
    }
}