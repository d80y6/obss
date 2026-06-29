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

    public CreditNote CreditNote { get; private set; } = null!;
}
