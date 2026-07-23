using FluentAssertions;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Obss.Provisioning.Infrastructure.Transports.Restconf.Model;
using Xunit;

namespace Obss.Provisioning.Tests.Transports.Restconf;

public class RestconfPathBuilderTests
{
    private readonly RestconfPathBuilder _builder = new("https://device/restconf");

    [Fact]
    public void DataPath_ShouldBuildCorrectUri()
    {
        var uri = _builder.DataPath("ietf-interfaces", "interfaces");
        uri.Should().Be("https://device/restconf/data/ietf-interfaces:interfaces");
    }

    [Fact]
    public void ListItemPath_ShouldBuildWithKey()
    {
        var uri = _builder.ListItemPath("ietf-interfaces", "interface", "GigabitEthernet0/0/0");
        uri.Should().Be("https://device/restconf/data/ietf-interfaces:interface=GigabitEthernet0%2F0%2F0");
    }

    [Fact]
    public void OperationPath_ShouldBuildCorrectUri()
    {
        var uri = _builder.OperationPath("ietf-system", "restart");
        uri.Should().Be("https://device/restconf/operations/ietf-system:restart");
    }

    [Fact]
    public void WithQueryParams_ShouldAppendDepth()
    {
        var uri = RestconfPathBuilder.WithQueryParams("/data/ietf-interfaces:interfaces", new RestconfQueryParams(Depth: 2));
        uri.Should().Be("/data/ietf-interfaces:interfaces?depth=2");
    }

    [Fact]
    public void WithQueryParams_ShouldAppendFields()
    {
        var uri = RestconfPathBuilder.WithQueryParams("/data/ietf-interfaces:interfaces", new RestconfQueryParams(Fields: "name;type"));
        uri.Should().Be("/data/ietf-interfaces:interfaces?fields=name%3Btype");
    }

    [Fact]
    public void WithQueryParams_ShouldAppendWithDefaults()
    {
        var uri = RestconfPathBuilder.WithQueryParams("/data/ietf-interfaces:interfaces", new RestconfQueryParams(WithDefaults: "report-all"));
        uri.Should().Be("/data/ietf-interfaces:interfaces?with-defaults=report-all");
    }

    [Fact]
    public void WithQueryParams_ShouldCombineMultiple()
    {
        var uri = RestconfPathBuilder.WithQueryParams("/data/ietf-interfaces:interfaces", new RestconfQueryParams(Depth: 2, Fields: "name"));
        uri.Should().Be("/data/ietf-interfaces:interfaces?depth=2&fields=name");
    }

    [Fact]
    public void DataPath_WithNestedContainer()
    {
        var uri = _builder.DataPath("ietf-interfaces", "interfaces/interface");
        uri.Should().Be("https://device/restconf/data/ietf-interfaces:interfaces/interface");
    }
}
