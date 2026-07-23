using FluentAssertions;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Xunit;

namespace Obss.Provisioning.Tests.Transports.Restconf;

public class RestconfErrorParserTests
{
    private readonly RestconfErrorParser _parser = new();

    [Fact]
    public void ParseError_Json_ShouldExtractErrorFields()
    {
        var json = @"{
            ""ietf-restconf:errors"": {
                ""error"": [{
                    ""error-type"": ""application"",
                    ""error-tag"": ""invalid-value"",
                    ""error-path"": ""/ietf-interfaces:interfaces/interface"",
                    ""error-message"": ""Invalid interface name""
                }]
            }
        }";
        var result = _parser.ParseError(json, "application/yang-data+json");
        result.Should().NotBeNull();
        result.ErrorType.Should().Be("application");
        result.ErrorTag.Should().Be("invalid-value");
        result.ErrorPath.Should().Be("/ietf-interfaces:interfaces/interface");
        result.ErrorMessage.Should().Be("Invalid interface name");
    }

    [Fact]
    public void ParseError_Json_NoErrors_ShouldReturnNull()
    {
        var json = "{\"ietf-interfaces:interfaces\":{\"interface\":[]}}";
        var result = _parser.ParseError(json, "application/yang-data+json");
        result.Should().BeNull();
    }

    [Fact]
    public void ParseError_InvalidJson_ShouldReturnNull()
    {
        var result = _parser.ParseError("not json", "application/yang-data+json");
        result.Should().BeNull();
    }

    [Fact]
    public void ParseError_NoErrorArray_ShouldReturnNull()
    {
        var json = "{\"ietf-restconf:errors\":{}}";
        var result = _parser.ParseError(json, "application/yang-data+json");
        result.Should().BeNull();
    }

    [Fact]
    public void ToString_ShouldFormatError()
    {
        var error = new RestconfErrorParser.ParsedError("application", "invalid-value", "/path", "bad value");
        error.ToString().Should().Contain("invalid-value").And.Contain("bad value");
    }

    [Fact]
    public void ParseError_EmptyErrorArray_ShouldReturnNull()
    {
        var json = "{\"ietf-restconf:errors\":{\"error\":[]}}";
        var result = _parser.ParseError(json, "application/yang-data+json");
        result.Should().BeNull();
    }

    [Fact]
    public void ParseError_MultipleErrors_ShouldReturnFirst()
    {
        var json = @"{
            ""ietf-restconf:errors"": {
                ""error"": [
                    {""error-type"":""protocol"",""error-tag"":""access-denied"",""error-message"":""Access denied""},
                    {""error-type"":""application"",""error-tag"":""invalid-value"",""error-message"":""Bad value""}
                ]
            }
        }";
        var result = _parser.ParseError(json, "application/yang-data+json");
        result.Should().NotBeNull();
        result.ErrorTag.Should().Be("access-denied");
    }
}