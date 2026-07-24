using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Nokia;
using Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Nokia;

public class NokiaRouterSimulatorTests
{
    private readonly NokiaRouterSimulator _simulator = new();

    public NokiaRouterSimulatorTests()
    {
        _simulator.ClearState();
    }

    [Fact]
    public async Task ConfigureInterface_And_GetInterface_ShouldRoundtrip()
    {
        var config = new InterfaceConfig("1/1/1", "Uplink", true, 1500, "100", "10.0.0.1", 24);
        var configureResult = await _simulator.ConfigureInterfaceAsync(config);
        configureResult.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetInterfaceAsync("1/1/1");
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.PortId.Should().Be("1/1/1");
    }

    [Fact]
    public async Task GetInterface_NotFound_ShouldReturnFailure()
    {
        var result = await _simulator.GetInterfaceAsync("nonexistent");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteInterface_ShouldRemove()
    {
        var config = new InterfaceConfig("1/1/1", "test", null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);

        var deleteResult = await _simulator.DeleteInterfaceAsync("1/1/1");
        deleteResult.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetInterfaceAsync("1/1/1");
        getResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetInterfaceStatuses_ShouldReturnList()
    {
        var config = new InterfaceConfig("1/1/1", "test", null, null, null, null, null);
        await _simulator.ConfigureInterfaceAsync(config);

        var result = await _simulator.GetInterfaceStatusesAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ConfigureBgp_And_GetBgp_ShouldRoundtrip()
    {
        var bgp = new BgpConfig(65001, "10.0.0.1", Array.Empty<BgpGroup>());
        var configureResult = await _simulator.ConfigureBgpAsync(bgp);
        configureResult.IsSuccess.Should().BeTrue();

        var getResult = await _simulator.GetBgpConfigAsync();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.AsNumber.Should().Be(65001);
    }

    [Fact]
    public async Task ConfigureStaticRoute_ShouldSucceed()
    {
        var route = new StaticRoute("0.0.0.0/0", "10.0.0.1", 1, null);
        var result = await _simulator.ConfigureStaticRouteAsync(route);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfigureIpFilter_ShouldSucceed()
    {
        var entries = new[] { new IpFilterEntry(10, "accept", "10.0.0.0/8", null, null, null, null, false) };
        var filter = new IpFilterConfig("filter-1", "Management", entries);
        var result = await _simulator.ConfigureIpFilterAsync(filter);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetDeviceStatus_ShouldReturnStatus()
    {
        var result = await _simulator.GetDeviceStatusAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Hostname.Should().Be("nokia-sros");
    }

    [Fact]
    public async Task GetInventory_ShouldReturnComponents()
    {
        var result = await _simulator.GetInventoryAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Model.Should().Be("7750 SR-12");
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnStatus()
    {
        var result = await _simulator.HealthCheckAsync();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveAlarms_ShouldReturnEmptyList()
    {
        var result = await _simulator.GetActiveAlarmsAsync();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FailureRate_ShouldSimulateErrors()
    {
        _simulator.FailureRate = 1.0;
        var result = await _simulator.GetDeviceStatusAsync();
        result.IsSuccess.Should().BeFalse();
    }
}
