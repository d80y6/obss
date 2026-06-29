using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Payments.Domain.Entities;

public class Refund : Entity<Guid>
{
    private Refund() { }

    public Refund(Guid id, Guid paymentId, decimal amount, string reason, DateTime createdAt)
        : base(id)
    {
        PaymentId = paymentId;
        Amount = amount;
        Reason = reason;
        Status = RefundStatus.Pending;
        CreatedAt = createdAt;
    }

    public Guid PaymentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public RefundStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public void Complete()
    {
        Status = RefundStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = RefundStatus.Failed;
    }
}
