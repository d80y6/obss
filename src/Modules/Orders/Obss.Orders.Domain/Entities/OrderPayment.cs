using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Orders.Domain.Entities;

public class OrderPayment : Entity<Guid>
{
    internal OrderPayment(
        Guid id,
        Guid orderId,
        decimal amount,
        string paymentMethod,
        string paymentReference,
        DateTime paidAt,
        PaymentStatus status)
        : base(id)
    {
        OrderId = orderId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        PaymentReference = paymentReference;
        PaidAt = paidAt;
        Status = status;
    }

    private OrderPayment() { }

    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string PaymentReference { get; private set; } = string.Empty;
    public DateTime PaidAt { get; private set; }
    public PaymentStatus Status { get; private set; }

    public void Complete()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot complete payment with status {Status}.");

        Status = PaymentStatus.Completed;
    }

    public void Fail()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot fail payment with status {Status}.");

        Status = PaymentStatus.Failed;
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Cannot refund a payment that has not been completed.");

        Status = PaymentStatus.Refunded;
    }
}
