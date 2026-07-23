using FluentAssertions;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Xunit;

namespace Obss.Provisioning.Tests.Transports.Restconf;

public class RestconfContentNegotiatorTests
{
    private readonly RestconfContentNegotiator _negotiator = new();

    [Fact]
    public void GetAcceptHeader_ShouldDefaultToJson()
    {
        var accept = _negotiator.GetAcceptHeader();
        accept.Should().Be("application/yang-data+json");
    }

    [Fact]
    public void GetAcceptHeader_WithXmlPreference_ShouldReturnXml()
    {
        var accept = _negotiator.GetAcceptHeader(preferXml: true);
        accept.Should().Be("application/yang-data+xml");
    }

    [Fact]
    public void Serialize_Json_ShouldProduceJsonString()
    {
        var body = new { name = "eth0", type = "ethernetCsmacd" };
        var json = RestconfContentNegotiator.Serialize(body, "application/yang-data+json");
        json.Should().NotBeNull();
        json.Should().Contain("eth0");
    }

    [Fact]
    public void Serialize_Xml_ShouldReturnXmlString()
    {
        var xml = "<interface><name>eth0</name></interface>";
        var result = RestconfContentNegotiator.Serialize(xml, "application/yang-data+xml");
        result.Should().Be(xml);
    }

    [Fact]
    public void Serialize_Xml_NonString_ShouldThrow()
    {
        Action act = () => RestconfContentNegotiator.Serialize(42, "application/yang-data+xml");
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void GetContentType_ShouldReturnJsonType()
    {
        _negotiator.GetContentType().Should().Be("application/yang-data+json");
    }

    [Fact]
    public void GetContentType_Xml_ShouldReturnXmlType()
    {
        var negotiator = new RestconfContentNegotiator(preferXml: true);
        negotiator.GetContentType().Should().Be("application/yang-data+xml");
    }

    [Fact]
    public void Serialize_WhenBodyIsNull_ShouldReturnEmptyString()
    {
        var result = RestconfContentNegotiator.Serialize(null, "application/yang-data+json");
        result.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_Json_ShouldReturnRawData()
    {
        var json = "{\"ietf-interfaces:interfaces\":{\"interface\":[{\"name\":\"eth0\"}]}}";
        var result = RestconfContentNegotiator.Deserialize(json, "application/yang-data+json");
        result.Should().Be(json);
    }

    [Fact]
    public void Deserialize_WhenDataIsNull_ShouldReturnNull()
    {
        var result = RestconfContentNegotiator.Deserialize(null!, "application/yang-data+json");
        result.Should().BeNull();
    }
}
