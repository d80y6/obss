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
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? ExternalId { get; private set; }
#pragma warning restore S1144
    public DateTime CreatedAt { get; private set; }
}
