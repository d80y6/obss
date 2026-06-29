using Obss.SharedKernel.Domain.Common;

namespace Obss.Payments.Domain.Entities;

public class PaymentAllocation : Entity<Guid>
{
    private PaymentAllocation() { }

    public PaymentAllocation(Guid id, Guid paymentId, Guid invoiceId, decimal amount, DateTime createdAt)
        : base(id)
    {
        PaymentId = paymentId;
        InvoiceId = invoiceId;
        Amount = amount;
        CreatedAt = createdAt;
    }

    public Guid PaymentId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
