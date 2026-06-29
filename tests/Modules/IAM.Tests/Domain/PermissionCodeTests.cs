using Xunit;
using FluentAssertions;
using Obss.IAM.Domain.ValueObjects;

namespace Obss.IAM.Tests.Domain;

public class PermissionCodeTests
{
    [Fact]
    public void Create_WithValidCode_ShouldSucceed()
    {
        var code = PermissionCode.Create("iam.user.read");

        code.Value.Should().Be("iam.user.read");
    }

    [Fact]
    public void Create_ShouldConvertToLowercase()
    {
        var code = PermissionCode.Create("IAM.USER.READ");

        code.Value.Should().Be("iam.user.read");
    }

    [Fact]
    public void Create_WithInvalidFormat_ShouldThrow()
    {
        var act = () => PermissionCode.Create("invalid");

        act.Should().Throw<ArgumentException>().WithMessage("*Invalid permission code format*");
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrow()
    {
        var act = () => PermissionCode.Create("");

        act.Should().Throw<ArgumentException>().WithMessage("*Permission code cannot be empty*");
    }

    [Fact]
    public void Create_WithWhiteSpace_ShouldThrow()
    {
        var act = () => PermissionCode.Create("   ");

        act.Should().Throw<ArgumentException>().WithMessage("*Permission code cannot be empty*");
    }

    [Fact]
    public void Create_WithMissingParts_ShouldThrow()
    {
        var act = () => PermissionCode.Create("iam.user");

        act.Should().Throw<ArgumentException>().WithMessage("*Invalid permission code format*");
    }

    [Fact]
    public void Create_WithExtraParts_ShouldThrow()
    {
        var act = () => PermissionCode.Create("iam.user.read.extra");

        act.Should().Throw<ArgumentException>().WithMessage("*Invalid permission code format*");
    }

    [Fact]
    public void Equality_TwoSameCodes_ShouldBeEqual()
    {
        var code1 = PermissionCode.Create("iam.user.read");
        var code2 = PermissionCode.Create("iam.user.read");

        code1.Should().Be(code2);
        code1.GetHashCode().Should().Be(code2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var code = PermissionCode.Create("iam.user.read");

        code.ToString().Should().Be("iam.user.read");
    }
}
