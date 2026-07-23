using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Cisco;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Cisco;

public class CiscoRouterSimulatorTests
{
    private readonly CiscoRouterSimulator _simulator = new();

    [Fact]
    public async Task ConfigureInterfaceAsync_ShouldStoreInterface()
    {
        var config = new InterfaceConfig("GigabitEthernet0/0/0", "GigabitEthernet",
            "Uplink", "10.0.0.1", 24, true, 1500, null, null);
        var result = await _simulator.ConfigureInterfaceAsync(config);
        result.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetInterfaceAsync("GigabitEthernet0/0/0");
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.Name.Should().Be("GigabitEthernet0/0/0");
        getResult.Data.IpAddress.Should().Be("10.0.0.1");
    }

    [Fact]
    public async Task GetInterfaceAsync_NonExistent_ShouldReturnFailure()
    {
        var result = await _simulator.GetInterfaceAsync("GigabitEthernet9/9/9");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteInterfaceAsync_ShouldRemoveInterface()
    {
        var config = new InterfaceConfig("GigabitEthernet0/0/1", "GigabitEthernet", null, null, null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);
        var deleteResult = await _simulator.DeleteInterfaceAsync("GigabitEthernet0/0/1");
        deleteResult.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetInterfaceAsync("GigabitEthernet0/0/1");
        getResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetInterfaceStatusesAsync_ShouldReturnAllInterfaces()
    {
        var config = new InterfaceConfig("GigabitEthernet0/0/0", "GigabitEthernet", null, null, null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);
        var result = await _simulator.GetInterfaceStatusesAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task ConfigureBgpAsync_ShouldStoreConfig()
    {
        var bgp = new BgpConfig(65001, "10.0.0.1", null, null);
        var result = await _simulator.ConfigureBgpAsync(bgp);
        result.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetBgpConfigAsync();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.AsNumber.Should().Be(65001);
    }

    [Fact]
    public async Task ConfigureSystemAsync_ShouldStoreConfig()
    {
        var sys = new SystemConfig("router01", "example.com", null, null, null);
        var result = await _simulator.ConfigureSystemAsync(sys);
        result.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetSystemConfigAsync();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.Hostname.Should().Be("router01");
    }

    [Fact]
    public async Task GetDeviceStatusAsync_ShouldReturnStatus()
    {
        var config = new InterfaceConfig("GigabitEthernet0/0/0", "GigabitEthernet", null, null, null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);
        var result = await _simulator.GetDeviceStatusAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Interfaces.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ConfigureStaticRouteAsync_ShouldStoreRoute()
    {
        var route = new StaticRoute("0.0.0.0/0", "10.0.0.1", 1, null);
        var result = await _simulator.ConfigureStaticRouteAsync(route);
        result.IsSuccess.Should().BeTrue();

        var routes = await _simulator.GetStaticRoutesAsync();
        routes.IsSuccess.Should().BeTrue();
        routes.Data.Should().Contain(r => r.Prefix == "0.0.0.0/0");
    }

    [Fact]
    public async Task ConfigureAclAsync_ShouldStoreAcl()
    {
        var entries = new[] { new AclEntry(10, "permit", "10.0.0.0", "0.0.0.255", null, null) };
        var acl = new AclConfig("MGMT-ACL", entries);
        var result = await _simulator.ConfigureAclAsync(acl);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveAlarmsAsync_ShouldReturnEmptyList()
    {
        var result = await _simulator.GetActiveAlarmsAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task ConfigureOspfAsync_ShouldStoreConfig()
    {
        var ospf = new OspfConfig(100, "10.0.0.1", null);
        var result = await _simulator.ConfigureOspfAsync(ospf);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetInventoryAsync_ShouldReturnDeviceInventory()
    {
        var result = await _simulator.GetInventoryAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Model.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ClearState_ShouldResetAllData()
    {
        var config = new InterfaceConfig("GigabitEthernet0/0/0", "GigabitEthernet", null, null, null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);

        _simulator.ClearState();

        var result = await _simulator.GetInterfaceAsync("GigabitEthernet0/0/0");
        result.IsSuccess.Should().BeFalse();
    }
}
