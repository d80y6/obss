using Xunit;
using FluentAssertions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Tests.Domain;

public class ProductOrderPaymentTests
{
    private static ProductOrder CreateOrderWithPayment(decimal amount = 100m, string method = "CreditCard", string reference = "PAY-001")
    {
        var order = ProductOrder.Create(
            "tenant-1", Guid.NewGuid(), "John Doe",
            OrderType.New, "user-1");
        order.AddPayment(amount, method, reference);
        return order;
    }

    [Fact]
    public void AddPayment_ShouldSetProperties()
    {
        var order = CreateOrderWithPayment(250.50m, "CreditCard", "PAY-001");
        var payment = order.Payments.Single();

        payment.OrderId.Should().Be(order.Id);
        payment.Amount.Should().Be(250.50m);
        payment.PaymentMethod.Should().Be("CreditCard");
        payment.PaymentReference.Should().Be("PAY-001");
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.PaidAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Complete_ShouldChangeStatusToCompleted()
    {
        var order = CreateOrderWithPayment();
        var payment = order.Payments.Single();

        payment.Complete();

        payment.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public void Complete_WhenNotPending_ShouldThrow()
    {
        var order = CreateOrderWithPayment();
        var payment = order.Payments.Single();
        payment.Complete();

        var act = () => payment.Complete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*complete payment*Completed*");
    }

    [Fact]
    public void Fail_ShouldChangeStatusToFailed()
    {
        var order = CreateOrderWithPayment();
        var payment = order.Payments.Single();

        payment.Fail();

        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void Fail_WhenNotPending_ShouldThrow()
    {
        var order = CreateOrderWithPayment();
        var payment = order.Payments.Single();
        payment.Complete();

        var act = () => payment.Fail();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*fail payment*Completed*");
    }

    [Fact]
    public void Refund_ShouldChangeStatusToRefunded()
    {
        var order = CreateOrderWithPayment();
        var payment = order.Payments.Single();
        payment.Complete();

        payment.Refund();

        payment.Status.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void Refund_WhenNotCompleted_ShouldThrow()
    {
        var order = CreateOrderWithPayment();
        var payment = order.Payments.Single();

        var act = () => payment.Refund();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not been completed*");
    }
}
