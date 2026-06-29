using Xunit;
using FluentAssertions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.SharedKernel.Tests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        FluentActions.Invoking(() => Money.Create(-100, "USD"))
            .Should().Throw<ArgumentException>()
            .WithMessage("*Amount cannot be negative*");
    }

    [Fact]
    public void Create_WithValidAmount_ShouldCreateMoney()
    {
        var money = Money.Create(100, "USD");

        money.Amount.Should().Be(100);
        money.Currency.Code.Should().Be("USD");
    }

    [Fact]
    public void Add_SameCurrency_ShouldReturnCorrectSum()
    {
        var money1 = Money.FromUsd(100);
        var money2 = Money.FromUsd(50);

        var result = money1.Add(money2);

        result.Amount.Should().Be(150);
        result.Currency.Code.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrow()
    {
        var money1 = Money.FromUsd(100);
        var money2 = Money.FromYer(50);

        FluentActions.Invoking(() => money1.Add(money2))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot add YER to USD*");
    }

    [Fact]
    public void Subtract_SameCurrency_ShouldReturnCorrectDifference()
    {
        var money1 = Money.FromUsd(100);
        var money2 = Money.FromUsd(30);

        var result = money1.Subtract(money2);

        result.Amount.Should().Be(70);
        result.Currency.Code.Should().Be("USD");
    }

    [Fact]
    public void Subtract_DifferentCurrency_ShouldThrow()
    {
        var money1 = Money.FromUsd(100);
        var money2 = Money.FromYer(30);

        FluentActions.Invoking(() => money1.Subtract(money2))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot subtract YER from USD*");
    }

    [Fact]
    public void Multiply_ShouldReturnCorrectProduct()
    {
        var money = Money.FromUsd(100);

        var result = money.Multiply(2.5m);

        result.Amount.Should().Be(250);
        result.Currency.Code.Should().Be("USD");
    }

    [Fact]
    public void Negate_ShouldReturnNegativeAmount()
    {
        var money = Money.FromUsd(100);

        var result = money.Negate();

        result.Amount.Should().Be(-100);
        result.Currency.Code.Should().Be("USD");
    }

    [Fact]
    public void Zero_ShouldHaveZeroAmountAndUsd()
    {
        Money.Zero.Amount.Should().Be(0);
        Money.Zero.Currency.Code.Should().Be("USD");
    }

    [Fact]
    public void FromUsd_ShouldCreateUsdMoney()
    {
        var money = Money.FromUsd(50);

        money.Currency.Code.Should().Be("USD");
        money.Currency.Name.Should().Be("US Dollar");
        money.Currency.NumericCode.Should().Be("840");
    }

    [Fact]
    public void FromYer_ShouldCreateYerMoney()
    {
        var money = Money.FromYer(50);

        money.Currency.Code.Should().Be("YER");
        money.Currency.Name.Should().Be("Yemeni Rial");
        money.Currency.NumericCode.Should().Be("886");
    }

    [Fact]
    public void Create_WithUnsupportedCurrency_ShouldThrow()
    {
        FluentActions.Invoking(() => Money.Create(100, "EUR"))
            .Should().Throw<ArgumentException>()
            .WithMessage("*Unsupported currency code*");
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("usd")]
    [InlineData("Usd")]
    public void FromCode_ShouldBeCaseInsensitive(string code)
    {
        var currency = Currency.FromCode(code);

        currency.Code.Should().Be("USD");
    }
}
