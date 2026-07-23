using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Cisco.Models;

public class CiscoModelTests
{
    [Fact]
    public void InterfaceConfig_ShouldPreserveAllProperties()
    {
        var config = new InterfaceConfig(
            "GigabitEthernet0/0/0", "GigabitEthernet",
            "Uplink", "10.0.0.1", 24, true, 1500, "100", new[] { "access" });
        config.Name.Should().Be("GigabitEthernet0/0/0");
        config.Type.Should().Be("GigabitEthernet");
        config.Description.Should().Be("Uplink");
        config.IpAddress.Should().Be("10.0.0.1");
        config.PrefixLength.Should().Be(24);
        config.AdminUp.Should().BeTrue();
        config.Mtu.Should().Be(1500);
        config.VlanId.Should().Be("100");
        config.SwitchportModes.Should().Contain("access");
    }

    [Fact]
    public void BgpConfig_ShouldPreserveProperties()
    {
        var neighbors = new[] { new BgpNeighbor("10.0.0.2", 65002, "ebgp") };
        var networks = new[] { new BgpNetwork("192.168.0.0", 24) };
        var bgp = new BgpConfig(65001, "10.0.0.1", neighbors, networks);
        bgp.AsNumber.Should().Be(65001);
        bgp.RouterId.Should().Be("10.0.0.1");
        bgp.Neighbors.Should().Contain(n => n.RemoteAs == 65002);
        bgp.Networks.Should().Contain(n => n.Prefix == "192.168.0.0");
    }

    [Fact]
    public void DeviceStatus_ShouldPreserveProperties()
    {
        var ifaces = new[] { new InterfaceStatus("GigabitEthernet0/0/0", "up", "up", 1000000) };
        var status = new DeviceStatus("router01", "17.9.1", "ISR4451", "100 days", 45.5, 60.2, ifaces);
        status.Hostname.Should().Be("router01");
        status.CpuUtilization.Should().Be(45.5);
        status.Interfaces.Should().Contain(i => i.OperStatus == "up");
    }

    [Fact]
    public void AclConfig_ShouldPreserveEntries()
    {
        var entries = new[] { new AclEntry(10, "permit", "10.0.0.0", "0.0.0.255", null, null) };
        var acl = new AclConfig("ACL-ALLOW-MGMT", entries);
        acl.Name.Should().Be("ACL-ALLOW-MGMT");
        acl.Entries.Should().Contain(e => e.Action == "permit");
    }

    [Fact]
    public void StaticRoute_ShouldPreserveProperties()
    {
        var route = new StaticRoute("0.0.0.0/0", "10.0.0.1", 1, "GigabitEthernet0/0/0");
        route.Prefix.Should().Be("0.0.0.0/0");
        route.NextHop.Should().Be("10.0.0.1");
        route.AdministrativeDistance.Should().Be(1);
    }

    [Fact]
    public void OspfConfig_ShouldPreserveProperties()
    {
        var areas = new[] { new OspfArea(0, new[] { "GigabitEthernet0/0/0" }) };
        var ospf = new OspfConfig(100, "10.0.0.1", areas);
        ospf.ProcessId.Should().Be(100);
        ospf.Areas.Should().Contain(a => a.AreaId == 0);
    }

    [Fact]
    public void SystemConfig_ShouldPreserveProperties()
    {
        var ntpServers = new[] { "10.0.0.10", "10.0.0.11" };
        var sys = new SystemConfig("router01", "example.com", ntpServers, new[] { "10.0.0.20" }, "obss-admin");
        sys.Hostname.Should().Be("router01");
        sys.NtpServers.Should().Contain("10.0.0.10");
    }

    [Fact]
    public void DeviceInventory_ShouldPreserveProperties()
    {
        var inv = new DeviceInventory("ISR4451-X/K9", "FTX2042AABB", "17.9.1", "2048MB", "16GB",
            new[] { new HardwareComponent("Power Supply 0", "PWR-4451-AC", "OK") });
        inv.Model.Should().Be("ISR4451-X/K9");
        inv.Components.Should().Contain(c => c.Status == "OK");
    }

    [Fact]
    public void AlarmInfo_ShouldPreserveProperties()
    {
        var alarm = new AlarmInfo("2026-07-23T10:00:00Z", "critical", "Interface down", "GigabitEthernet0/0/0");
        alarm.Severity.Should().Be("critical");
        alarm.Description.Should().Be("Interface down");
    }

    [Fact]
    public void BgpNeighbor_ShouldPreserveProperties()
    {
        var neighbor = new BgpNeighbor("10.0.0.2", 65002, "ebgp");
        neighbor.RemoteAddress.Should().Be("10.0.0.2");
        neighbor.RemoteAs.Should().Be(65002);
    }

    [Fact]
    public void InterfaceStatus_ShouldPreserveProperties()
    {
        var status = new InterfaceStatus("GigabitEthernet0/0/0", "up", "up", 1000000);
        status.InterfaceName.Should().Be("GigabitEthernet0/0/0");
        status.AdminStatus.Should().Be("up");
        status.OperStatus.Should().Be("up");
        status.Speed.Should().Be(1000000);
    }
}