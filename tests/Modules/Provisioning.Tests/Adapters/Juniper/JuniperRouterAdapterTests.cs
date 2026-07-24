using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Juniper;
using Obss.Provisioning.Infrastructure.Adapters.Juniper.Models;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Juniper;

[CollectionDefinition("JuniperAdapter", DisableParallelization = true)]
public class JuniperAdapterTestCollection { }

[Collection("JuniperAdapter")]
public sealed class JuniperRouterAdapterTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly JuniperRouterAdapter _adapter;

    public JuniperRouterAdapterTests()
    {
        _server = WireMockServer.Start();
        var config = new JuniperAdapterConfig { BaseUri = _server.Url! };
        var transport = new RestconfTransport(new RestconfTransportConfig
        {
            BaseUri = _server.Url!
        });
        _adapter = new JuniperRouterAdapter(config, transport);
    }

    [Fact]
    public async Task ConfigureInterfaceAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/JunOS:configuration/interfaces/interface/name=Loopback0").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var config = new InterfaceConfig("Loopback0", "Management", "0", null, "10.0.0.1", 24, true, 1500);
        var result = await _adapter.ConfigureInterfaceAsync(config);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetInterfaceAsync_ShouldReturnInterface()
    {
        _server.Given(Request.Create().WithPath("/data/JunOS:configuration/interfaces/interface/name=Loopback0").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"interface\":{\"name\":\"Loopback0\",\"description\":\"Management\"}}"));

        var result = await _adapter.GetInterfaceAsync("Loopback0");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetInterfaceAsync_NotFound_ShouldReturnFailure()
    {
        _server.Given(Request.Create().WithPath("/data/JunOS:configuration/interfaces/interface/name=Nonexistent").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(404));

        var result = await _adapter.GetInterfaceAsync("Nonexistent");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteInterfaceAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/JunOS:configuration/interfaces/interface/name=Loopback0").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(204));

        var result = await _adapter.DeleteInterfaceAsync("Loopback0");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfigureBgpAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/JunOS:configuration/protocols/bgp").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var bgp = new BgpConfig(65001, "10.0.0.1", Array.Empty<BgpGroup>());
        var result = await _adapter.ConfigureBgpAsync(bgp);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetBgpConfigAsync_ShouldReturnConfig()
    {
        _server.Given(Request.Create().WithPath("/data/JunOS:configuration/protocols/bgp").UsingGet())
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
        _server.Given(Request.Create().WithPath("/data/JunOS:configuration/routing-options/static/route").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201));

        var route = new StaticRoute("0.0.0.0/0", "10.0.0.1", 1, null, null);
        var result = await _adapter.ConfigureStaticRouteAsync(route);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfigureFirewallFilterAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/JunOS:configuration/firewall/filter/name=protect").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var terms = new[] { new FirewallFilterTerm("term1", "accept", "10.0.0.0/8", null, null, null, null, false) };
        var filter = new FirewallFilterConfig("protect", terms);
        var result = await _adapter.ConfigureFirewallFilterAsync(filter);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetDeviceStatusAsync_ShouldReturnStatus()
    {
        _server.Given(Request.Create().WithPath("/data/JunOS:system-information").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"system-information\":{\"hostname\":\"router01\",\"version\":\"21.2R1\",\"model\":\"MX204\"}}"));

        var result = await _adapter.GetDeviceStatusAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Hostname.Should().Be("router01");
    }

    [Fact]
    public async Task GetActiveAlarmsAsync_ShouldReturnAlarms()
    {
        _server.Given(Request.Create().WithPath("/data/JunOS:alarm-information").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"alarm-information\":{\"alarm\":[{\"severity\":\"critical\",\"alarm-description\":\"Interface down\",\"alarm-source\":\"ge-0/0/0\"}]}}"));

        var result = await _adapter.GetActiveAlarmsAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain(a => a.Severity == "critical");
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnStatus()
    {
        _server.Given(Request.Create().WithPath("/data/JunOS:system-information").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"system-information\":{\"hostname\":\"router01\",\"version\":\"21.2R1\",\"model\":\"MX204\"}}"));

        var result = await _adapter.HealthCheckAsync();
        result.IsSuccess.Should().BeTrue();
    }

    public void Dispose()
    {
        _server?.Dispose();
        _adapter?.Dispose();
    }
}
