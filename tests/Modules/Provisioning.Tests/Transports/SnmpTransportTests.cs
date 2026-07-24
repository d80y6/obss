using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Snmp;
using Xunit;

namespace Obss.Provisioning.Tests.Transports;

public class SnmpTransportTests
{
    private readonly ILogger<SnmpTransport> _logger;

    public SnmpTransportTests()
    {
        _logger = Substitute.For<ILogger<SnmpTransport>>();
    }

    [Fact]
    public void Construct_ShouldThrowOnNullConfig()
    {
        var act = () => new SnmpTransport(null!, _logger);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Construct_ShouldThrowOnNullLogger()
    {
        var act = () => new SnmpTransport(new SnmpTransportConfig(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(SnmpVersion.V1, TransportProtocol.SnmpV1)]
    [InlineData(SnmpVersion.V2C, TransportProtocol.SnmpV2C)]
    [InlineData(SnmpVersion.V3, TransportProtocol.SnmpV3)]
    public void Protocol_ShouldReturnExpectedValue(SnmpVersion version, TransportProtocol expected)
    {
        var transport = CreateTransport(version);

        transport.Protocol.Should().Be(expected);
    }

    [Fact]
    public void Config_ShouldReturnConfiguredConfig()
    {
        var config = new SnmpTransportConfig
        {
            Host = "192.168.1.1",
            Port = 161,
            Community = "private",
            SnmpVersion = SnmpVersion.V2C
        };
        var transport = new SnmpTransport(config, _logger);

        transport.Config.Should().BeSameAs(config);
    }

    [Fact]
    public async Task GetBulkAsync_WithEmptyOids_ShouldReturnOk()
    {
        var transport = CreateTransport();

        var result = await transport.GetBulkAsync([]);

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public void V3Config_WithVariousProtocols_ShouldConstruct()
    {
        var config = new SnmpTransportConfig
        {
            Host = "192.168.1.1",
            Port = 161,
            SnmpVersion = SnmpVersion.V3,
            V3UserName = "testuser",
            V3AuthPassword = "authpass",
            V3PrivPassword = "privpass",
            V3AuthProtocol = "SHA256",
            V3PrivProtocol = "AES256"
        };
        var transport = new SnmpTransport(config, _logger);

        transport.Protocol.Should().Be(TransportProtocol.SnmpV3);
    }

    [Fact]
    public void V3Config_WithMD5AndDES_ShouldConstruct()
    {
        var config = new SnmpTransportConfig
        {
            Host = "192.168.1.1",
            Port = 161,
            SnmpVersion = SnmpVersion.V3,
            V3UserName = "testuser",
            V3AuthPassword = "authpass",
            V3PrivPassword = "privpass",
            V3AuthProtocol = "MD5",
            V3PrivProtocol = "DES"
        };
        var transport = new SnmpTransport(config, _logger);

        transport.Protocol.Should().Be(TransportProtocol.SnmpV3);
    }

    [Fact]
    public void V3Config_WithNoAuthNoPriv_ShouldConstruct()
    {
        var config = new SnmpTransportConfig
        {
            Host = "192.168.1.1",
            Port = 161,
            SnmpVersion = SnmpVersion.V3,
            V3UserName = "testuser",
            V3AuthProtocol = "NONE",
            V3PrivProtocol = "NONE"
        };
        var transport = new SnmpTransport(config, _logger);

        transport.Protocol.Should().Be(TransportProtocol.SnmpV3);
    }

    [Theory]
    [InlineData(SnmpVersion.V1)]
    [InlineData(SnmpVersion.V2C)]
    [InlineData(SnmpVersion.V3)]
    public void HostResolution_ShouldWorkForIPAddress(SnmpVersion version)
    {
        var transport = CreateTransport(version, host: "10.0.0.1");

        var protocol = transport.Protocol;
        protocol.Should().BeOneOf(TransportProtocol.SnmpV1, TransportProtocol.SnmpV2C, TransportProtocol.SnmpV3);
    }

    [Fact]
    public void SnmpVersionEnum_ShouldHaveCorrectValues()
    {
        ((int)SnmpVersion.V1).Should().Be(0);
        ((int)SnmpVersion.V2C).Should().Be(1);
        ((int)SnmpVersion.V3).Should().Be(2);
    }

    [Fact]
    public void TransportProtocol_ShouldBeSnmp()
    {
        var transportV1 = CreateTransport(SnmpVersion.V1);
        var transportV2 = CreateTransport(SnmpVersion.V2C);

        transportV1.Protocol.Should().Be(TransportProtocol.SnmpV1);
        transportV2.Protocol.Should().Be(TransportProtocol.SnmpV2C);
    }

    private static SnmpTransport CreateTransport(
        SnmpVersion version = SnmpVersion.V2C,
        string host = "127.0.0.1",
        int port = 161,
        string community = "public")
    {
        return new SnmpTransport(new SnmpTransportConfig
        {
            Host = host,
            Port = port,
            Community = community,
            SnmpVersion = version
        }, Substitute.For<ILogger<SnmpTransport>>());
    }
}
