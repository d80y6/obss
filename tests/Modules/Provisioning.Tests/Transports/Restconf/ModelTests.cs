using FluentAssertions;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;
using Xunit;

namespace Obss.Provisioning.Tests.Transports.Restconf;

public class RestconfModelTests
{
    [Fact]
    public void RestconfResult_Ok_ShouldSetSuccess()
    {
        var result = RestconfResult.Ok("{\"data\":\"value\"}", protocol: TransportProtocol.Restconf);
        result.Success.Should().BeTrue();
        result.Data.Should().Be("{\"data\":\"value\"}");
        result.Protocol.Should().Be(TransportProtocol.Restconf);
    }

    [Fact]
    public void RestconfResult_Fail_ShouldSetError()
    {
        var result = RestconfResult.Fail("error occurred", protocol: TransportProtocol.Restconf);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("error occurred");
    }

    [Fact]
    public void RestconfResource_Enums_ShouldHaveExpectedValues()
    {
        ((int)RestconfResourceType.Datastore).Should().Be(1);
        ((int)RestconfResourceType.Operation).Should().Be(2);
        ((int)RestconfResourceType.Stream).Should().Be(3);
    }

    [Fact]
    public void YangModule_ShouldPreserveAllProperties()
    {
        var module = new YangModule("ietf-interfaces", "2018-02-20",
            "urn:ietf:params:xml:ns:yang:ietf-interfaces",
            "https://example.com/schema", new[] { "feature-a" },
            new[] { "ietf-interfaces-deviations" });
        module.Name.Should().Be("ietf-interfaces");
        module.Revision.Should().Be("2018-02-20");
        module.Namespace.Should().Be("urn:ietf:params:xml:ns:yang:ietf-interfaces");
        module.SchemaUri.Should().Be("https://example.com/schema");
        module.Features.Should().Contain("feature-a");
        module.Deviations.Should().Contain("ietf-interfaces-deviations");
    }
}