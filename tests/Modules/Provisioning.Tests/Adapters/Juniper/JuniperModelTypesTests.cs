using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Juniper;

public class JuniperModelTypesTests
{
    [Fact]
    public void InterfaceConfig_CanBeConstructed()
    {
        var sut = new InterfaceConfig("ge-0/0/0", "Uplink", "0", 100, "10.0.0.1", 24, true, 1500);
        sut.Name.Should().Be("ge-0/0/0");
        sut.IpAddress.Should().Be("10.0.0.1");
    }

    [Fact]
    public void BgpConfig_CanBeConstructed()
    {
        var sut = new BgpConfig(65001, "10.0.0.1", Array.Empty<BgpGroup>());
        sut.AsNumber.Should().Be(65001);
    }

    [Fact]
    public void FirewallFilterConfig_CanBeConstructed()
    {
        var terms = new[] { new FirewallFilterTerm("term1", "accept", "10.0.0.0/8", null, null, null, null, false) };
        var sut = new FirewallFilterConfig("protect", terms);
        sut.Name.Should().Be("protect");
    }

    [Fact]
    public void DeviceInventory_CanBeConstructed()
    {
        var comps = new[] { new HardwareComponent("FPC0", "MX204", "ABC123", "Line Card", "Rev1", "750-12345") };
        var sut = new DeviceInventory("MX204", "SN123", "750-12345", "Router", "21.2R1", comps);
        sut.Model.Should().Be("MX204");
    }

    [Fact]
    public void AlarmInfo_CanBeConstructed()
    {
        var sut = new AlarmInfo("1", "critical", "Interface down", "ge-0/0/0", DateTime.UtcNow);
        sut.Severity.Should().Be("critical");
    }
}
