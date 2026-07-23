using FluentAssertions;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Obss.Provisioning.Tests.Transports.Restconf;

public sealed class RestconfTransportTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly RestconfTransport _transport;
    private readonly RestconfTransportConfig _config;

    public RestconfTransportTests()
    {
        _server = WireMockServer.Start();
        _config = new RestconfTransportConfig { BaseUri = _server.Url! };
        _transport = new RestconfTransport(_config);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnData()
    {
        _server.Given(Request.Create().WithPath("/data/ietf-interfaces:interfaces").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/yang-data+json")
                .WithBody("{\"ietf-interfaces:interfaces\":{\"interface\":[]}}"));

        var result = await _transport.GetAsync("/data/ietf-interfaces:interfaces");

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_ErrorResponse_ShouldReturnFailure()
    {
        _server.Given(Request.Create().WithPath("/data/ietf-interfaces:interface=bad").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithHeader("Content-Type", "application/yang-data+json")
                .WithBody("{\"ietf-restconf:errors\":{\"error\":[{\"error-type\":\"application\",\"error-tag\":\"invalid-value\",\"error-message\":\"Bad value\"}]}}"));

        var result = await _transport.GetAsync("/data/ietf-interfaces:interface=bad");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("invalid-value");
    }

    [Fact]
    public async Task PostAsync_ShouldReturnSuccess()
    {
        _server.Given(Request.Create().WithPath("/data/ietf-interfaces:interfaces").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithHeader("Content-Type", "application/yang-data+json"));

        var result = await _transport.PostAsync("/data/ietf-interfaces:interfaces", new { name = "eth1" });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess()
    {
        _server.Given(Request.Create().WithPath("/data/ietf-interfaces:interface=eth0").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(204));

        var result = await _transport.DeleteAsync("/data/ietf-interfaces:interface=eth0");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetYangLibraryAsync_ShouldParseModuleList()
    {
        _server.Given(Request.Create().WithPath("/modules-state/module").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/yang-data+json")
                .WithBody("{\"ietf-yang-library:modules-state\":{\"module\":[{\"name\":\"ietf-interfaces\",\"namespace\":\"urn:ietf:params:xml:ns:yang:ietf-interfaces\"}]}}"));

        var lib = await _transport.GetYangLibraryAsync();

        lib.Should().NotBeNull();
        lib.Modules.Should().Contain(m => m.Name == "ietf-interfaces");
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldSucceed()
    {
        _server.Given(Request.Create().UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/yang-data+json")
                .WithBody("{\"ietf-restconf:restconf\":{}}"));

        var result = await _transport.TestConnectionAsync();

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PutAsync_ShouldReturnSuccess()
    {
        _server.Given(Request.Create().WithPath("/data/ietf-interfaces:interface=eth0").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/yang-data+json"));

        var result = await _transport.PutAsync("/data/ietf-interfaces:interface=eth0", new { name = "eth0", type = "ethernetCsmacd" });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PatchAsync_ShouldReturnSuccess()
    {
        _server.Given(Request.Create().WithPath("/data/ietf-interfaces:interface=eth0").UsingPatch())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/yang-data+json"));

        var result = await _transport.PatchAsync("/data/ietf-interfaces:interface=eth0", new { description = "Updated" });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_ServerError_ShouldReturnFailure()
    {
        _server.Given(Request.Create().WithPath("/data/ietf-interfaces:interfaces").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));

        var result = await _transport.GetAsync("/data/ietf-interfaces:interfaces");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("500");
    }

    public void Dispose()
    {
        _server?.Dispose();
        _transport?.Dispose();
    }
}
