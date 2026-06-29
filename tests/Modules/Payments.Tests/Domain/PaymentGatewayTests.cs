using Xunit;
using FluentAssertions;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Tests.Domain;

public class PaymentGatewayTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var gateway = PaymentGateway.Create(
            "tenant-1", "Stripe Gateway", PaymentProvider.Stripe,
            "{\"key\":\"sk_test\"}", new[] { "USD", "EUR" },
            10m, 10000m, 2.5m, FeeType.Percentage);

        gateway.Name.Should().Be("Stripe Gateway");
        gateway.Provider.Should().Be(PaymentProvider.Stripe);
        gateway.IsActive.Should().BeTrue();
        gateway.SupportedCurrencies.Should().Contain(["USD", "EUR"]);
        gateway.TransactionFee.Should().Be(2.5m);
        gateway.FeeType.Should().Be(FeeType.Percentage);
    }

    [Fact]
    public void Activate_ShouldSetActive()
    {
        var gateway = CreateGateway();
        gateway.Deactivate();

        gateway.Activate();

        gateway.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetInactive()
    {
        var gateway = CreateGateway();

        gateway.Deactivate();

        gateway.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateConfiguration_ShouldUpdate()
    {
        var gateway = CreateGateway();

        gateway.UpdateConfiguration("{\"key\":\"new_key\"}");

        gateway.Configuration.Should().Be("{\"key\":\"new_key\"}");
    }

    private static PaymentGateway CreateGateway()
    {
        return PaymentGateway.Create(
            "tenant-1", "PayPal Gateway", PaymentProvider.PayPal,
            "{\"client_id\":\"abc\"}", new[] { "USD" },
            null, null, 3.5m, FeeType.Fixed);
    }
}
