using FluentAssertions;
using Obss.SharedKernel.Application.Contracts;
using Xunit;

namespace Obss.SharedKernel.Tests.Application.Contracts;

public class TmfResourceReferenceTests
{
    [Fact]
    public void Create_WithGuid_Should_SetProperties()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var ref_ = TmfResourceReference.Create("customer", id);

        ref_.Id.Should().Be("11111111-1111-1111-1111-111111111111");
        ref_.Href.Should().Be("/api/v1/customer/11111111-1111-1111-1111-111111111111");
        ref_.AtType.Should().Be("Customer");
        ref_.SchemaLocation.Should().Be("https://tmf-open-api.net/schemas/customer.json");
    }

    [Fact]
    public void Create_WithString_Should_UseProvidedId()
    {
        var ref_ = TmfResourceReference.Create("order", "ORD-20240101-001");

        ref_.Id.Should().Be("ORD-20240101-001");
        ref_.Href.Should().Be("/api/v1/order/ORD-20240101-001");
        ref_.AtType.Should().Be("Order");
    }

    [Fact]
    public void Create_WithCustomVersion_Should_UseVersionInHref()
    {
        var id = Guid.NewGuid();
        var ref_ = TmfResourceReference.Create("product", id, "v2");

        ref_.Href.Should().Contain("/api/v2/product/");
    }
}
