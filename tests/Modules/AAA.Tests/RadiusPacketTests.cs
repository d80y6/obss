using System.Net;
using System.Text;
using Xunit;
using FluentAssertions;
using Obss.AAA.Infrastructure.Services.Radius;

namespace Obss.AAA.Tests;

public class RadiusPacketTests
{
    [Fact]
    public void Encode_AccessRequest_ShouldBuildValidPacket()
    {
        var packet = new RadiusPacket
        {
            Code = RadiusCode.AccessRequest,
            Identifier = 1,
            Authenticator = new byte[16],
        };

        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserName, Encoding.UTF8.GetBytes("testuser")));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.NasIpAddress, IPAddress.Parse("192.168.1.1").GetAddressBytes()));

        var data = packet.Encode("shared-secret");

        data.Should().HaveCountGreaterThanOrEqualTo(20);
        data[0].Should().Be(1);
        data[1].Should().Be(1);
    }

    [Fact]
    public void Encode_AccountingRequest_ShouldBuildValidPacket()
    {
        var packet = new RadiusPacket
        {
            Code = RadiusCode.AccountingRequest,
            Identifier = 5,
            Authenticator = new byte[16],
        };

        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.AcctStatusType, new byte[] { 0, 0, 0, 1 }));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.AcctSessionId, Encoding.UTF8.GetBytes("session-001")));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserName, Encoding.UTF8.GetBytes("testuser")));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.NasIpAddress, IPAddress.Parse("10.0.0.1").GetAddressBytes()));

        var data = packet.Encode("test-secret-123");

        data[0].Should().Be(4);
        data[1].Should().Be(5);
    }

    [Fact]
    public void Parse_AccountingResponse_ShouldSucceed()
    {
        const string secret = "test-secret";

        var request = new RadiusPacket
        {
            Code = RadiusCode.AccountingRequest,
            Identifier = 10,
            Authenticator = new byte[16],
        };
        request.Attributes.Add(new RadiusAttribute(RadiusAttributeType.AcctStatusType, new byte[] { 0, 0, 0, 2 }));
        request.Attributes.Add(new RadiusAttribute(RadiusAttributeType.AcctSessionId, Encoding.UTF8.GetBytes("test-session")));
        request.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserName, Encoding.UTF8.GetBytes("testuser")));

        var requestData = request.Encode(secret);

        var parsedResponse = RadiusPacket.Parse(requestData, secret);
        parsedResponse.Should().NotBeNull();
    }

    [Fact]
    public void Parse_TruncatedPacket_ShouldThrow()
    {
        var data = new byte[5];

        var act = () => RadiusPacket.Parse(data, "secret");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Length_ShouldReflectAttributes()
    {
        var packet = new RadiusPacket
        {
            Code = RadiusCode.AccessRequest,
            Identifier = 0,
            Authenticator = new byte[16],
        };

        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserName, Encoding.UTF8.GetBytes("user")));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserPassword, Encoding.UTF8.GetBytes("pass")));

        packet.Length.Should().Be(32);
    }

    [Fact]
    public void GetStringAttribute_ShouldReturnCorrectValue()
    {
        var packet = new RadiusPacket
        {
            Code = RadiusCode.AccessAccept,
            Identifier = 0,
        };

        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.ReplyMessage, Encoding.UTF8.GetBytes("Welcome!")));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.FramedIpAddress, IPAddress.Parse("10.0.0.100").GetAddressBytes()));

        packet.GetStringAttribute(RadiusAttributeType.ReplyMessage).Should().Be("Welcome!");
        packet.GetIpAttribute(RadiusAttributeType.FramedIpAddress).Should().Be(IPAddress.Parse("10.0.0.100"));
    }

    [Fact]
    public void GetUintAttribute_ShouldDecodeCorrectly()
    {
        var packet = new RadiusPacket
        {
            Code = RadiusCode.AccessAccept,
            Identifier = 0,
        };

        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.SessionTimeout, new byte[] { 0, 0, 0x0E, 0x10 }));

        packet.GetUintAttribute(RadiusAttributeType.SessionTimeout).Should().Be(3600);
    }
}
