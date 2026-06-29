using Xunit;
using FluentAssertions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.SharedKernel.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldCreateEmail()
    {
        var email = Email.Create("user@example.com");

        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_WithValidEmail_ShouldTrimAndLowercase()
    {
        var email = Email.Create("  User@Example.COM  ");

        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_WithMissingAtSymbol_ShouldThrow()
    {
        FluentActions.Invoking(() => Email.Create("userexample.com"))
            .Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email format*");
    }

    [Fact]
    public void Create_WithMissingDot_ShouldThrow()
    {
        FluentActions.Invoking(() => Email.Create("user@example"))
            .Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email format*");
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrow()
    {
        FluentActions.Invoking(() => Email.Create(""))
            .Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldThrow()
    {
        FluentActions.Invoking(() => Email.Create("   "))
            .Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void Create_WithNull_ShouldThrow()
    {
        FluentActions.Invoking(() => Email.Create(null!))
            .Should().Throw<ArgumentException>()
            .WithMessage("*Email cannot be empty*");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var email = Email.Create("test@example.com");

        email.ToString().Should().Be("test@example.com");
    }

    [Fact]
    public void TwoEmailsWithSameValue_ShouldBeEqual()
    {
        var email1 = Email.Create("user@example.com");
        var email2 = Email.Create("user@example.com");

        (email1 == email2).Should().BeTrue();
        email1.Equals(email2).Should().BeTrue();
    }

    [Fact]
    public void TwoEmailsWithDifferentValues_ShouldNotBeEqual()
    {
        var email1 = Email.Create("user1@example.com");
        var email2 = Email.Create("user2@example.com");

        (email1 != email2).Should().BeTrue();
    }
}
