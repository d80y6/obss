using Xunit;
using FluentAssertions;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.Exceptions;
using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Tests.Domain;

public class PaymentTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var payment = Payment.Create("tenant-1", "PAY-000001", Guid.NewGuid(), 500m, "USD", PaymentMethodType.BankTransfer);

        payment.TenantId.Should().Be("tenant-1");
        payment.PaymentNumber.Should().Be("PAY-000001");
        payment.Amount.Should().Be(500m);
        payment.Currency.Should().Be("USD");
        payment.PaymentMethod.Should().Be(PaymentMethodType.BankTransfer);
        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Complete_ShouldSetStatusToCompleted()
    {
        var payment = Payment.Create("tenant-1", "PAY-000001", Guid.NewGuid(), 500m, "USD", PaymentMethodType.Cash);

        payment.Complete();

        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrow()
    {
        var payment = Payment.Create("tenant-1", "PAY-000001", Guid.NewGuid(), 500m, "USD", PaymentMethodType.Cash);
        payment.Complete();

        var act = () => payment.Complete();

        act.Should().Throw<PaymentAlreadyCompletedException>();
    }

    [Fact]
    public void Complete_WhenRefunded_ShouldThrow()
    {
        var payment = CreateCompletedPayment();
        payment.Refund(200m, "Customer request");

        var act = () => payment.Complete();

        act.Should().Throw<InvalidPaymentStateException>();
    }

    [Fact]
    public void Fail_ShouldSetStatusToFailed()
    {
        var payment = Payment.Create("tenant-1", "PAY-000001", Guid.NewGuid(), 500m, "USD", PaymentMethodType.Cash);

        payment.Fail("Insufficient funds");

        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.Notes.Should().Be("Insufficient funds");
    }

    [Fact]
    public void Fail_WhenAlreadyCompleted_ShouldThrow()
    {
        var payment = CreateCompletedPayment();

        var act = () => payment.Fail("reason");

        act.Should().Throw<PaymentAlreadyCompletedException>();
    }

    [Fact]
    public void Refund_ShouldReduceAmount()
    {
        var payment = CreateCompletedPayment();

        payment.Refund(200m, "Customer request");

        payment.Status.Should().Be(PaymentStatus.PartiallyRefunded);
        payment.Refunds.Should().ContainSingle(r => r.Amount == 200m);
    }

    [Fact]
    public void Refund_FullAmount_ShouldSetRefunded()
    {
        var payment = CreateCompletedPayment();

        payment.Refund(500m, "Full refund");

        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.Refunds.Should().ContainSingle(r => r.Amount == 500m);
    }

    [Fact]
    public void Refund_ExceedingAvailableAmount_ShouldThrow()
    {
        var payment = CreateCompletedPayment();

        var act = () => payment.Refund(600m, "Over refund");

        act.Should().Throw<InsufficientPaymentAmountException>();
    }

    [Fact]
    public void Refund_WhenNotCompleted_ShouldThrow()
    {
        var payment = Payment.Create("tenant-1", "PAY-000001", Guid.NewGuid(), 500m, "USD", PaymentMethodType.Cash);

        var act = () => payment.Refund(100m, "Not allowed");

        act.Should().Throw<InvalidPaymentStateException>();
    }

    [Fact]
    public void AllocateToInvoice_ShouldAddAllocation()
    {
        var payment = CreateCompletedPayment();
        var invoiceId = Guid.NewGuid();

        payment.AllocateToInvoice(invoiceId, 200m);

        payment.Allocations.Should().ContainSingle(a => a.InvoiceId == invoiceId && a.Amount == 200m);
    }

    [Fact]
    public void AllocateToInvoice_WhenExceedingAvailable_ShouldThrow()
    {
        var payment = CreateCompletedPayment();

        var act = () => payment.AllocateToInvoice(Guid.NewGuid(), 600m);

        act.Should().Throw<InsufficientPaymentAmountException>();
    }

    [Fact]
    public void AllocateToInvoice_WhenNotCompleted_ShouldThrow()
    {
        var payment = Payment.Create("tenant-1", "PAY-000001", Guid.NewGuid(), 500m, "USD", PaymentMethodType.Cash);

        var act = () => payment.AllocateToInvoice(Guid.NewGuid(), 100m);

        act.Should().Throw<InvalidPaymentStateException>();
    }

    private static Payment CreateCompletedPayment()
    {
        var payment = Payment.Create("tenant-1", "PAY-000001", Guid.NewGuid(), 500m, "USD", PaymentMethodType.Cash);
        payment.Complete();
        return payment;
    }
}
