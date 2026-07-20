using FluentAssertions;
using Obss.Provisioning.Infrastructure.Adapters.Huawei;
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
        config.SnmpCommunity.Should().BeNull();
        config.SnmpPort.Should().Be(161);
        config.DeviceModel.Should().BeNull();
        config.ControllerProfile.Should().BeNull();
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
            SnmpCommunity = "public",
            SnmpPort = 1161,
            DeviceModel = "MA5800-X17",
            ControllerProfile = "access",
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
        config.SnmpCommunity.Should().Be("public");
        config.SnmpPort.Should().Be(1161);
        config.DeviceModel.Should().Be("MA5800-X17");
        config.ControllerProfile.Should().Be("access");
    }
}
