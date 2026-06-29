using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Payments.Domain.Entities;

public class ReconciliationItem : Entity<Guid>
{
    private ReconciliationItem() { }

    public ReconciliationItem(
        Guid id,
        Guid reconciliationId,
        string externalReference,
        decimal amount,
        string currency,
        DateTime transactionDate,
        string? description)
        : base(id)
    {
        ReconciliationId = reconciliationId;
        ExternalReference = externalReference;
        Amount = amount;
        Currency = currency;
        TransactionDate = transactionDate;
        Description = description;
        Status = ReconciliationItemStatus.Unmatched;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ReconciliationId { get; private set; }
    public string ExternalReference { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }
    public string? Description { get; private set; }
    public Guid? MatchedInvoiceId { get; private set; }
    public Guid? MatchedPaymentId { get; private set; }
    public ReconciliationItemStatus Status { get; private set; }
    public string? DiscrepancyReason { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void Match(Guid invoiceId, Guid paymentId)
    {
        MatchedInvoiceId = invoiceId;
        MatchedPaymentId = paymentId;
        Status = ReconciliationItemStatus.Matched;
        DiscrepancyReason = null;
    }

    public void MarkAsDiscrepancy(string reason)
    {
        Status = ReconciliationItemStatus.Discrepancy;
        DiscrepancyReason = reason;
        MatchedInvoiceId = null;
        MatchedPaymentId = null;
    }
}
