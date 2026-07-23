using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Cisco;
using Obss.Provisioning.Infrastructure.Adapters.Cisco.Models;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Cisco;

[CollectionDefinition("CiscoAdapter", DisableParallelization = true)]
public class CiscoAdapterTestCollection;

[Collection("CiscoAdapter")]
public sealed class CiscoRouterAdapterTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly CiscoRouterAdapter _adapter;

    public CiscoRouterAdapterTests()
    {
        _server = WireMockServer.Start();
        var restconfTransport = new RestconfTransport(new RestconfTransportConfig
        {
            BaseUri = _server.Url!
        });
        _adapter = new CiscoRouterAdapter(new CiscoAdapterConfig { BaseUri = _server.Url! }, restconfTransport);
    }

    [Fact]
    public async Task ConfigureInterfaceAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native/interface/GigabitEthernet=0/0/0").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var config = new InterfaceConfig("GigabitEthernet0/0/0", "GigabitEthernet",
            "Uplink", "10.0.0.1", 24, true, 1500, null, null);
        var result = await _adapter.ConfigureInterfaceAsync(config);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetInterfaceAsync_ShouldReturnInterface()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native/interface/GigabitEthernet=0/0/0").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Cisco-IOS-XE-native:GigabitEthernet\":{\"name\":\"0/0/0\",\"description\":\"Uplink\"}}"));

        var result = await _adapter.GetInterfaceAsync("GigabitEthernet0/0/0");
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be("GigabitEthernet0/0/0");
    }

    [Fact]
    public async Task GetInterfaceAsync_NotFound_ShouldReturnFailure()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native/interface/*").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(404));

        var result = await _adapter.GetInterfaceAsync("GigabitEthernet9/9/9");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetInterfaceStatusesAsync_ShouldReturnList()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Cisco-IOS-XE-native:native\":{\"interface\":{\"GigabitEthernet\":[{\"name\":\"0/0/0\"}]}}}"));

        var result = await _adapter.GetInterfaceStatusesAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteInterfaceAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native/interface/GigabitEthernet=0/0/0").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(204));

        var result = await _adapter.DeleteInterfaceAsync("GigabitEthernet0/0/0");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfigureBgpAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-bgp:bgp").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var bgp = new BgpConfig(65001, "10.0.0.1", null, null);
        var result = await _adapter.ConfigureBgpAsync(bgp);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetDeviceStatusAsync_ShouldReturnStatus()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Cisco-IOS-XE-native:native\":{\"hostname\":\"router01\",\"version\":\"17.9.1\"}}"));

        var result = await _adapter.GetDeviceStatusAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Hostname.Should().Be("router01");
    }

    [Fact]
    public async Task ConfigureStaticRouteAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native/ip/route").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201));

        var route = new StaticRoute("0.0.0.0/0", "10.0.0.1", 1, null);
        var result = await _adapter.ConfigureStaticRouteAsync(route);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveAlarmsAsync_ShouldReturnAlarms()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-alarms:alarms").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Cisco-IOS-XE-alarms:alarms\":{\"alarm\":[{\"severity\":\"critical\",\"alarm-description\":\"Interface down\"}]}}"));

        var result = await _adapter.GetActiveAlarmsAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain(a => a.Severity == "critical");
    }

    [Fact]
    public async Task GetBgpConfigAsync_ShouldReturnBgpConfig()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-bgp:bgp").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Cisco-IOS-XE-bgp:bgp\":{\"id\":65001,\"routerId\":\"10.0.0.1\"}}"));

        var result = await _adapter.GetBgpConfigAsync();
        result.IsSuccess.Should().BeTrue();
        result.Data.AsNumber.Should().Be(65001);
    }

    [Fact]
    public async Task ConfigureSystemAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().WithPath("/data/Cisco-IOS-XE-native:native").UsingPut())
            .RespondWith(Response.Create().WithStatusCode(201));

        var sys = new SystemConfig("router01", "example.com", null, null, null);
        var result = await _adapter.ConfigureSystemAsync(sys);
        result.IsSuccess.Should().BeTrue();
    }

    public void Dispose()
    {
        _server?.Dispose();
        _adapter?.Dispose();
    }
}
