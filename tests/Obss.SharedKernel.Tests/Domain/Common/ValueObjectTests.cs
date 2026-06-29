using Xunit;
using FluentAssertions;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.SharedKernel.Tests.Domain.Common;

public class ValueObjectTests
{
    private sealed class TestValueObject : ValueObject
    {
        public TestValueObject(string value1, int value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        public string Value1 { get; }
        public int Value2 { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value1;
            yield return Value2;
        }
    }

    [Fact]
    public void TwoValueObjectsWithSameComponents_ShouldBeEqual()
    {
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 42);

        (vo1 == vo2).Should().BeTrue();
        vo1.Equals(vo2).Should().BeTrue();
        vo1.GetHashCode().Should().Be(vo2.GetHashCode());
    }

    [Fact]
    public void TwoValueObjectsWithDifferentComponents_ShouldNotBeEqual()
    {
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("different", 42);

        (vo1 != vo2).Should().BeTrue();
        vo1.Equals(vo2).Should().BeFalse();
    }

    [Fact]
    public void ValueObject_ShouldNotBeEqual_WhenComparedToDifferentType()
    {
        var vo = new TestValueObject("test", 42);

        vo.Equals("string").Should().BeFalse();
        vo.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Money_WithSameAmountAndCurrency_ShouldBeEqual()
    {
        var money1 = Money.FromUsd(100);
        var money2 = Money.FromUsd(100);

        (money1 == money2).Should().BeTrue();
        money1.Equals(money2).Should().BeTrue();
    }

    [Fact]
    public void Money_WithDifferentAmount_ShouldNotBeEqual()
    {
        var money1 = Money.FromUsd(100);
        var money2 = Money.FromUsd(200);

        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Money_WithDifferentCurrency_ShouldNotBeEqual()
    {
        var money1 = Money.FromUsd(100);
        var money2 = Money.FromYer(100);

        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Email_CreationAndEquality_ShouldWork()
    {
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");
        var email3 = Email.Create("other@example.com");

        (email1 == email2).Should().BeTrue();
        (email1 != email3).Should().BeTrue();
    }

    [Fact]
    public void Email_ShouldNormalizeToLowercase()
    {
        var email = Email.Create("Test@Example.COM");

        email.Value.Should().Be("test@example.com");
    }
}
