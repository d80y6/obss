using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Entities;

public class CreditNoteLine : Entity<Guid>
{
    private CreditNoteLine() { }

    public CreditNoteLine(
        Guid id,
        Guid creditNoteId,
        Guid invoiceLineId,
        string description,
        decimal amount,
        decimal quantity)
        : base(id)
    {
        CreditNoteId = creditNoteId;
        InvoiceLineId = invoiceLineId;
        Description = description;
        Amount = amount;
        Quantity = quantity;
    }

    public Guid CreditNoteId { get; private set; }
    public Guid InvoiceLineId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public decimal Quantity { get; private set; }
#pragma warning disable S1144 // Used by EF Core via reflection
    public string? ExternalId { get; private set; }
#pragma warning restore S1144

    public CreditNote CreditNote { get; private set; } = null!;
}
