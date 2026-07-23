using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Cisco;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters.Cisco;

public class CiscoAdapterConfigTests
{
    [Fact]
    public void CiscoAdapterConfig_ShouldSetDefaultValues()
    {
        var config = new CiscoAdapterConfig { BaseUri = "https://device/restconf" };
        config.BaseUri.Should().Be("https://device/restconf");
        config.ControllerUrl.Should().BeNull();
        config.TimeoutSeconds.Should().Be(30);
        config.MaxRetries.Should().Be(3);
    }

    [Fact]
    public void CiscoAdapterConfig_ShouldOverrideDefaults()
    {
        var config = new CiscoAdapterConfig
        {
            BaseUri = "https://device/restconf",
            TimeoutSeconds = 60,
            MaxRetries = 5
        };
        config.TimeoutSeconds.Should().Be(60);
        config.MaxRetries.Should().Be(5);
    }
}
