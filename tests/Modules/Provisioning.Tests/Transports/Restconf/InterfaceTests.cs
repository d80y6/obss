using FluentAssertions;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Xunit;

namespace Obss.Provisioning.Tests.Transports.Restconf;

public class RestconfInterfaceTests
{
    [Fact]
    public void RestconfTransportConfig_ShouldSetProtocol()
    {
        var config = new RestconfTransportConfig { BaseUri = "https://device/restconf" };
        config.Protocol.Should().Be(TransportProtocol.Restconf);
        config.BaseUri.Should().Be("https://device/restconf");
    }

    [Fact]
    public void RestconfTransportConfig_ShouldDefaultCacheTtl()
    {
        var config = new RestconfTransportConfig();
        config.YangLibraryCacheTtlSeconds.Should().Be(300);
    }
}
