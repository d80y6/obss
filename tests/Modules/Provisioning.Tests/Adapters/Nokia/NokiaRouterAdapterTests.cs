using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Nokia;
using Obss.Provisioning.Infrastructure.Adapters.Nokia.Models;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Nokia;

[CollectionDefinition("NokiaAdapter", DisableParallelization = true)]
public class NokiaAdapterTestCollection { }

[Collection("NokiaAdapter")]
public sealed class NokiaRouterAdapterTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly NokiaRouterAdapter _adapter;

    public NokiaRouterAdapterTests()
    {
        _server = WireMockServer.Start();
        var config = new NokiaAdapterConfig { BaseUri = _server.Url! };
        var transport = new RestconfTransport(new RestconfTransportConfig
        {
            BaseUri = _server.Url!
        });
        _adapter = new NokiaRouterAdapter(config, transport);
    }

    [Fact]
    public async Task ConfigureInterfaceAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/nokia-conf:configure/port/port-id=Loopback0").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var config = new InterfaceConfig("Loopback0", "Management", true, 1500, null, "10.0.0.1", 24);
        var result = await _adapter.ConfigureInterfaceAsync(config);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetInterfaceAsync_ShouldReturnInterface()
    {
        _server.Given(Request.Create().WithPath("/data/nokia-conf:configure/port/port-id=Loopback0").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"port\":{\"portId\":\"Loopback0\",\"description\":\"Management\"}}"));

        var result = await _adapter.GetInterfaceAsync("Loopback0");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetInterfaceAsync_NotFound_ShouldReturnFailure()
    {
        _server.Given(Request.Create().WithPath("/data/nokia-conf:configure/port/port-id=Nonexistent").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(404));

        var result = await _adapter.GetInterfaceAsync("Nonexistent");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteInterfaceAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/nokia-conf:configure/port/port-id=Loopback0").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(204));

        var result = await _adapter.DeleteInterfaceAsync("Loopback0");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfigureBgpAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/nokia-conf:configure/router/0/bgp").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var bgp = new BgpConfig(65001, "10.0.0.1", Array.Empty<BgpGroup>());
        var result = await _adapter.ConfigureBgpAsync(bgp);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetBgpConfigAsync_ShouldReturnConfig()
    {
        _server.Given(Request.Create().WithPath("/data/nokia-conf:configure/router/0/bgp").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"bgp\":{\"group\":[{\"name\":\"EBGP\",\"type\":\"external\",\"peerAs\":65002}]}}"));

        var result = await _adapter.GetBgpConfigAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Groups.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ConfigureStaticRouteAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/nokia-conf:configure/router/0/static-route").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201));

        var route = new StaticRoute("0.0.0.0/0", "10.0.0.1", 1, null);
        var result = await _adapter.ConfigureStaticRouteAsync(route);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfigureIpFilterAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/nokia-conf:configure/ip-filter/name=filter-1").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var entries = new[] { new IpFilterEntry(10, "accept", "10.0.0.0/8", null, null, null, null, false) };
        var filter = new IpFilterConfig("filter-1", "Management", entries);
        var result = await _adapter.ConfigureIpFilterAsync(filter);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetDeviceStatusAsync_ShouldReturnStatus()
    {
        _server.Given(Request.Create().WithPath("/data/nokia-state:state/system").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"system\":{\"name\":\"nokia-rtr01\",\"version\":\"20.10.R1\"}}"));

        var result = await _adapter.GetDeviceStatusAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Hostname.Should().Be("nokia-rtr01");
    }

    [Fact]
    public async Task GetActiveAlarmsAsync_ShouldReturnAlarms()
    {
        _server.Given(Request.Create().WithPath("/data/nokia-state:state/alarm").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"alarm\":[{\"severity\":\"critical\",\"description\":\"Port down\",\"source\":\"1/1/1\"}]}"));

        var result = await _adapter.GetActiveAlarmsAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain(a => a.Severity == "critical");
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnStatus()
    {
        _server.Given(Request.Create().WithPath("/data/nokia-state:state/system").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"system\":{\"name\":\"nokia-rtr01\",\"version\":\"20.10.R1\"}}"));

        var result = await _adapter.HealthCheckAsync();
        result.IsSuccess.Should().BeTrue();
    }

    public void Dispose()
    {
        _server?.Dispose();
        _adapter?.Dispose();
    }
}
