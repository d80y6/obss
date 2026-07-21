using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Huawei;
using Obss.Provisioning.Infrastructure.Transports.Snmp;
using Xunit;

namespace Obss.Provisioning.Tests.Adapters;

public class HuaweiAdapterConfigTests
{
    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        var config = new HuaweiAdapterConfig();

        config.ControllerUrl.Should().BeNull();
        config.Username.Should().BeNull();
        config.Password.Should().BeNull();
        config.ValidateCertificate.Should().BeTrue();
        config.TimeoutSeconds.Should().Be(30);
        config.MaxRetries.Should().Be(3);
        config.RetryDelayMs.Should().Be(1000);
        config.EnableCircuitBreaker.Should().BeFalse();
        config.CircuitBreakerThreshold.Should().Be(5);
        config.CircuitBreakerDuration.Should().Be(TimeSpan.FromMinutes(5));
        config.UseSimulator.Should().BeFalse();
        config.SnmpTransport.Should().BeNull();
        config.SshTransport.Should().BeNull();
        config.NetconfTransport.Should().BeNull();
        config.RestTransport.Should().BeNull();
    }

    [Fact]
    public void ShouldSetAndGetAllProperties()
    {
        var config = new HuaweiAdapterConfig
        {
            ControllerUrl = "https://huawei-olt:8443",
            Username = "admin",
            Password = "secret",
            ValidateCertificate = false,
            TimeoutSeconds = 60,
            MaxRetries = 5,
            RetryDelayMs = 2000,
            EnableCircuitBreaker = true,
            CircuitBreakerThreshold = 10,
            CircuitBreakerDuration = TimeSpan.FromMinutes(10),
            UseSimulator = true,
            SnmpTransport = new SnmpTransportConfig
            {
                Host = "10.0.0.1",
                Port = 161,
                Community = "public"
            }
        };

        config.ControllerUrl.Should().Be("https://huawei-olt:8443");
        config.Username.Should().Be("admin");
        config.Password.Should().Be("secret");
        config.ValidateCertificate.Should().BeFalse();
        config.TimeoutSeconds.Should().Be(60);
        config.MaxRetries.Should().Be(5);
        config.RetryDelayMs.Should().Be(2000);
        config.EnableCircuitBreaker.Should().BeTrue();
        config.CircuitBreakerThreshold.Should().Be(10);
        config.CircuitBreakerDuration.Should().Be(TimeSpan.FromMinutes(10));
        config.UseSimulator.Should().BeTrue();
        config.SnmpTransport.Should().NotBeNull();
        config.SnmpTransport!.Host.Should().Be("10.0.0.1");
        config.SnmpTransport.Community.Should().Be("public");
    }

    [Fact]
    public void TryGetMethods_ShouldReturnNullWhenSimulatorEnabled()
    {
        var config = new HuaweiAdapterConfig
        {
            UseSimulator = true,
            SnmpTransport = new SnmpTransportConfig { Host = "10.0.0.1" }
        };

        config.TryGetSnmpConfig().Should().BeNull();
    }

    [Fact]
    public void TryGetMethods_ShouldReturnConfigWhenSimulatorDisabled()
    {
        var config = new HuaweiAdapterConfig
        {
            UseSimulator = false,
            SnmpTransport = new SnmpTransportConfig { Host = "10.0.0.1" }
        };

        config.TryGetSnmpConfig().Should().NotBeNull();
        config.TryGetSnmpConfig()!.Host.Should().Be("10.0.0.1");
    }
}
