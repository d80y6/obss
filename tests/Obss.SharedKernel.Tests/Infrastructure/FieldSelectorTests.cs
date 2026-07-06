using System.Text.Json;
using FluentAssertions;
using Obss.SharedKernel.Infrastructure;
using Xunit;

namespace Obss.SharedKernel.Tests.Infrastructure;

public class FieldSelectorTests
{
    [Fact]
    public void SelectFields_WithValidFields_ShouldFilterProperties()
    {
        var source = new { Id = 1, Name = "Test", Description = "Desc", Price = 10.0m };
        var result = FieldSelector.SelectFields(source, "id,name");

        result.Should().Contain("\"id\":1");
        result.Should().Contain("\"name\":\"Test\"");
        result.Should().NotContain("description");
        result.Should().NotContain("price");
    }

    [Fact]
    public void SelectFields_WhenNull_ShouldReturnAll()
    {
        var source = new { Id = 1, Name = "Test" };
        var result = FieldSelector.SelectFields(source, null);

        using var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("id", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("name", out _).Should().BeTrue();
    }

    [Fact]
    public void SelectFields_WhenEmpty_ShouldReturnAll()
    {
        var source = new { Id = 1, Name = "Test" };
        var result = FieldSelector.SelectFields(source, "");

        using var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("id", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("name", out _).Should().BeTrue();
    }

    [Fact]
    public void SelectFields_WithUnknownField_ShouldIgnore()
    {
        var source = new { Id = 1, Name = "Test" };
        var result = FieldSelector.SelectFields(source, "id,unknownField");

        using var json = JsonDocument.Parse(result);
        json.RootElement.TryGetProperty("id", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("unknownField", out _).Should().BeFalse();
        json.RootElement.TryGetProperty("name", out _).Should().BeFalse();
    }
}
