using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Transports;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Extensions;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Xunit;

namespace Obss.Provisioning.Tests.Transports.Restconf;

public class RestconfFactoryIntegrationTests
{
    [Fact]
    public void TransportFactory_ShouldSupportRestconf()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNetworkTransports();
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<ITransportFactory>();

        factory.SupportsProtocol(TransportProtocol.Restconf).Should().BeTrue();
    }

    [Fact]
    public void TransportFactory_ShouldCreateRestconfTransport()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNetworkTransports();
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<ITransportFactory>();

        var config = new RestconfTransportConfig
        {
            BaseUri = "https://device/restconf"
        };

        var transport = factory.CreateTransport(config);
        transport.Should().BeOfType<RestconfTransport>();
        transport.Protocol.Should().Be(TransportProtocol.Restconf);
    }

    private sealed record InvalidRestconfConfig : TransportConfigBase
    {
        public override TransportProtocol Protocol => TransportProtocol.Restconf;
    }

    [Fact]
    public void TransportFactory_CreateRestconf_WithInvalidConfig_ShouldThrow()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNetworkTransports();
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<ITransportFactory>();

        var invalidConfig = new InvalidRestconfConfig();

        Action act = () => factory.CreateTransport(invalidConfig);
        act.Should().Throw<InvalidOperationException>().WithMessage("*RestconfTransportConfig*");
    }

    [Fact]
    public void AddRestconfTransport_ShouldRegisterSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var config = new RestconfTransportConfig { BaseUri = "https://device/restconf" };
        services.AddRestconfTransport(config);

        var sp = services.BuildServiceProvider();
        var transport = sp.GetRequiredService<IRestconfTransport>();
        transport.Should().BeOfType<RestconfTransport>();
        transport.Protocol.Should().Be(TransportProtocol.Restconf);
    }
}
