using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Juniper;
using Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Juniper;

public class JuniperRouterSimulatorTests
{
    private readonly JuniperRouterSimulator _simulator = new();

    [Fact]
    public async Task ConfigureInterfaceAsync_ShouldStoreInterface()
    {
        var config = new InterfaceConfig("ge-0/0/0", "Uplink", "0", 100, "10.0.0.1", 24, true, 1500);
        var result = await _simulator.ConfigureInterfaceAsync(config);
        result.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetInterfaceAsync("ge-0/0/0");
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.Name.Should().Be("ge-0/0/0");
        getResult.Data.IpAddress.Should().Be("10.0.0.1");
    }

    [Fact]
    public async Task GetInterfaceAsync_NonExistent_ShouldReturnFailure()
    {
        var result = await _simulator.GetInterfaceAsync("ge-9/9/9");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteInterfaceAsync_ShouldRemoveInterface()
    {
        var config = new InterfaceConfig("ge-0/0/1", "Test", "0", null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);
        var deleteResult = await _simulator.DeleteInterfaceAsync("ge-0/0/1");
        deleteResult.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetInterfaceAsync("ge-0/0/1");
        getResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetInterfaceStatusesAsync_ShouldReturnAllInterfaces()
    {
        var config = new InterfaceConfig("ge-0/0/0", "Uplink", "0", null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);
        var result = await _simulator.GetInterfaceStatusesAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task ConfigureBgpAsync_ShouldStoreConfig()
    {
        var bgp = new BgpConfig(65001, "10.0.0.1", Array.Empty<BgpGroup>());
        var result = await _simulator.ConfigureBgpAsync(bgp);
        result.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetBgpConfigAsync();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.AsNumber.Should().Be(65001);
    }

    [Fact]
    public async Task ConfigureStaticRouteAsync_ShouldStoreRoute()
    {
        var route = new StaticRoute("0.0.0.0/0", "10.0.0.1", 1, null, null);
        var result = await _simulator.ConfigureStaticRouteAsync(route);
        result.IsSuccess.Should().BeTrue();

        var routes = await _simulator.GetStaticRoutesAsync();
        routes.IsSuccess.Should().BeTrue();
        routes.Data.Should().Contain(r => r.Prefix == "0.0.0.0/0");
    }

    [Fact]
    public async Task ConfigureFirewallFilterAsync_ShouldStoreFilter()
    {
        var terms = new[] { new FirewallFilterTerm("term1", "accept", "10.0.0.0/8", null, null, null, null, false) };
        var filter = new FirewallFilterConfig("protect", terms);
        var result = await _simulator.ConfigureFirewallFilterAsync(filter);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetDeviceStatusAsync_ShouldReturnStatus()
    {
        var config = new InterfaceConfig("ge-0/0/0", "Uplink", "0", null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);
        var result = await _simulator.GetDeviceStatusAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Interfaces.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetInventoryAsync_ShouldReturnDeviceInventory()
    {
        var result = await _simulator.GetInventoryAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Model.Should().Be("MX204");
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnDeviceStatus()
    {
        var result = await _simulator.HealthCheckAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Hostname.Should().Be("junos-router");
    }

    [Fact]
    public async Task GetActiveAlarmsAsync_ShouldReturnEmptyList()
    {
        var result = await _simulator.GetActiveAlarmsAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task FailureRate_ShouldCauseFailures()
    {
        _simulator.FailureRate = 1.0;
        var result = await _simulator.GetInterfaceAsync("ge-0/0/0");
        result.IsSuccess.Should().BeFalse();
    }
}
