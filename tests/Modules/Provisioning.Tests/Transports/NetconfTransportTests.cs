using System.Text;
using System.Xml.Linq;
using FluentAssertions;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Netconf;
using Xunit;

namespace Obss.Provisioning.Tests.Transports;

public class NetconfTransportTests
{
    [Fact]
    public void FrameMessage_ShouldProduceRfc6242ChunkedFraming()
    {
        var payload = "<rpc-reply><data><test/></data></rpc-reply>";
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        var framed = NetconfTransport.FrameMessage(payloadBytes);
        var framedStr = Encoding.UTF8.GetString(framed);

        framedStr.Should().StartWith($"\n#{payloadBytes.Length}\n");
        framedStr.Should().Contain(payload);
        framedStr.Should().EndWith("\n##\n");
    }

    [Fact]
    public void FrameMessage_ShouldHandleEmptyMessage()
    {
        var framed = NetconfTransport.FrameMessage(Array.Empty<byte>());
        var framedStr = Encoding.UTF8.GetString(framed);

        framedStr.Should().Be("\n#0\n\n##\n");
    }

    [Fact]
    public void FrameMessage_ShouldHandleLargeMessage()
    {
        var payload = new string('x', 100000);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        var framed = NetconfTransport.FrameMessage(payloadBytes);
        var framedStr = Encoding.UTF8.GetString(framed);

        framedStr.Should().StartWith($"\n#{payloadBytes.Length}\n");
        framedStr.Should().EndWith("\n##\n");
    }

    [Fact]
    public void CleanNetconfResponse_ShouldExtractRpcReplyFromChunkedResponse()
    {
        var response = "\n#145\n<rpc-reply message-id=\"101\" xmlns=\"urn:ietf:params:xml:ns:netconf:base:1.0\">\n  <data>\n    <system>\n      <hostname>router1</hostname>\n    </system>\n  </data>\n</rpc-reply>\n##\n";

        var cleaned = NetconfTransport.CleanNetconfResponse(response);

        cleaned.Should().Contain("<rpc-reply");
        cleaned.Should().Contain("<data>");
        cleaned.Should().Contain("<hostname>router1</hostname>");
        cleaned.Should().NotContain("\n##\n");
    }

    [Fact]
    public void CleanNetconfResponse_ShouldReturnRawContent_WhenXmlParsingFails()
    {
        var response = "\n#10\n<invalid>\n##\n";

        var cleaned = NetconfTransport.CleanNetconfResponse(response);

        cleaned.Should().Be("<invalid>");
    }

    [Fact]
    public void CleanNetconfResponse_ShouldHandleMultiChunkResponse()
    {
        var chunk1 = "\n#50\n<rpc-reply message-id=\"1\" xmlns=\"urn:ietf:params:xml:ns:netconf:base:1.0\">\n  <data>\n    <interfaces>\n";
        var chunk2 = "\n#60\n      <interface>\n        <name>GigabitEthernet0/0/0</name>\n        <status>up</status>\n      </interface>\n";
        var chunk3 = "\n#20\n    </interfaces>\n  </data>\n</rpc-reply>\n##\n";
        var response = chunk1 + chunk2 + chunk3;

        var cleaned = NetconfTransport.CleanNetconfResponse(response);

        cleaned.Should().Contain("<data>");
        cleaned.Should().Contain("GigabitEthernet0/0/0");
        cleaned.Should().Contain("<name>");
    }

    [Fact]
    public void CleanNetconfResponse_ShouldHandleResponseWithoutChunking()
    {
        var response = "<rpc-reply xmlns=\"urn:ietf:params:xml:ns:netconf:base:1.0\"><data><ok/></data></rpc-reply>";

        var cleaned = NetconfTransport.CleanNetconfResponse(response);

        cleaned.Should().Contain("rpc-reply");
        cleaned.Should().Contain("<data");
        cleaned.Should().Contain("<ok");
        cleaned.Should().NotContain("##");
    }

    [Fact]
    public void CleanNetconfResponse_ShouldHandleRpcError()
    {
        var response = "\n#200\n<rpc-reply message-id=\"1\" xmlns=\"urn:ietf:params:xml:ns:netconf:base:1.0\">\n  <rpc-error>\n    <error-type>protocol</error-type>\n    <error-tag>operation-failed</error-tag>\n    <error-severity>error</error-severity>\n    <error-message>Access denied</error-message>\n  </rpc-error>\n</rpc-reply>\n##\n";

        var cleaned = NetconfTransport.CleanNetconfResponse(response);

        cleaned.Should().Contain("<rpc-error>");
        cleaned.Should().Contain("Access denied");
    }

    [Fact]
    public void CleanNetconfResponse_ShouldReturnEmpty_WhenOnlyEomMarker()
    {
        var cleaned = NetconfTransport.CleanNetconfResponse("\n##\n");

        cleaned.Should().BeEmpty();
    }

    [Fact]
    public void IsErrorResponse_ShouldDetectRpcError()
    {
        NetconfTransport.IsErrorResponse("<rpc-reply><rpc-error><error-message>timeout</error-message></rpc-error></rpc-reply>")
            .Should().BeTrue();
    }

    [Fact]
    public void IsErrorResponse_ShouldReturnFalseForSuccessResponse()
    {
        NetconfTransport.IsErrorResponse("<rpc-reply><data><ok/></data></rpc-reply>")
            .Should().BeFalse();
    }

    [Fact]
    public void ExtractError_ShouldExtractErrorMessage()
    {
        var errorXml = "<rpc-reply xmlns=\"urn:ietf:params:xml:ns:netconf:base:1.0\"><rpc-error><error-message>Access denied</error-message></rpc-error></rpc-reply>";

        var result = NetconfTransport.ExtractError(errorXml);

        result.Should().Be("Access denied");
    }

    [Fact]
    public void ExtractError_ShouldReturnDefault_WhenNoError()
    {
        NetconfTransport.ExtractError("<rpc-reply><data><ok/></data></rpc-reply>")
            .Should().Be("Unknown NETCONF error");
    }

    [Fact]
    public void Construct_ShouldThrowOnNullConfig()
    {
        var act = () => new NetconfTransport(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Protocol_ShouldReturnNetconf()
    {
        var transport = CreateTransport();

        transport.Protocol.Should().Be(TransportProtocol.Netconf);
    }

    [Fact]
    public void Config_ShouldReturnConfiguredConfig()
    {
        var config = new NetconfTransportConfig
        {
            Host = "192.168.1.1",
            Port = 830,
            Username = "admin",
            Password = "secret",
            TimeoutSeconds = 30
        };
        var transport = new NetconfTransport(config);

        transport.Config.Should().BeSameAs(config);
    }

    [Fact]
    public async Task TestConnectionAsync_WithInvalidHost_ShouldReturnFail()
    {
        var transport = new NetconfTransport(new NetconfTransportConfig
        {
            Host = "192.0.2.1",
            Port = 830,
            Username = "test",
            Password = "test",
            TimeoutSeconds = 2
        });

        var result = await transport.TestConnectionAsync();

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage!.ToLower().Should().Contain("fail");
    }

    [Fact]
    public void HuaweiNceSampleFixture_ParseSucceeds()
    {
        var fixture = "\n#450\n<rpc-reply message-id=\"1\" xmlns=\"urn:ietf:params:xml:ns:netconf:base:1.0\">\n  <data>\n    <ifm xmlns=\"http://www.huawei.com/netconf/vrp\">\n      <interfaces>\n        <interface>\n          <ifName>GigabitEthernet0/1/0</ifName>\n          <ifPhyType>GE</ifPhyType>\n          <ifMtu>1500</ifMtu>\n        </interface>\n      </interfaces>\n    </ifm>\n  </data>\n</rpc-reply>\n##\n";

        var cleaned = NetconfTransport.CleanNetconfResponse(fixture);

        cleaned.Should().Contain("<data>");
        cleaned.Should().Contain("<ifm");
        cleaned.Should().Contain("GigabitEthernet0/1/0");
        cleaned.Should().Contain("</rpc-reply>");
        cleaned.Should().NotContain("##");
    }

    private static NetconfTransport CreateTransport()
    {
        return new NetconfTransport(new NetconfTransportConfig
        {
            Host = "localhost",
            Port = 830,
            Username = "test",
            Password = "test",
            TimeoutSeconds = 5
        });
    }
}
