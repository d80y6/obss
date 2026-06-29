using Xunit;
using FluentAssertions;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Tests.Domain;

public class PaymentMethodTests
{
    [Fact]
    public void Constructor_ShouldMaskAccountNumber()
    {
        var method = new PaymentMethod(Guid.NewGuid(), "tenant-1", Guid.NewGuid(), PaymentMethodType.CreditCard, "Visa", "1234567890123456", null);

        method.AccountNumber.Should().Be("************3456");
        method.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithShortAccountNumber_ShouldNotMask()
    {
        var method = new PaymentMethod(Guid.NewGuid(), "tenant-1", Guid.NewGuid(), PaymentMethodType.Cash, "Cash", "12", null);

        method.AccountNumber.Should().Be("12");
    }

    [Fact]
    public void Activate_ShouldSetIsActive()
    {
        var method = CreateMethod();
        method.Deactivate();

        method.Activate();

        method.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetInactive()
    {
        var method = CreateMethod();

        method.Deactivate();

        method.IsActive.Should().BeFalse();
    }

    [Fact]
    public void SetAsDefault_ShouldSetDefault()
    {
        var method = CreateMethod();

        method.SetAsDefault();

        method.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void UnsetDefault_ShouldUnsetDefault()
    {
        var method = CreateMethod();
        method.SetAsDefault();

        method.UnsetDefault();

        method.IsDefault.Should().BeFalse();
    }

    private static PaymentMethod CreateMethod()
    {
        return new PaymentMethod(Guid.NewGuid(), "tenant-1", Guid.NewGuid(), PaymentMethodType.BankTransfer, "Bank", "9876543210987654", null);
    }
}
