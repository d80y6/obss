using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Events;

public sealed class CreditNoteIssuedDomainEvent : DomainEvent, INotification
{
    public CreditNoteIssuedDomainEvent(Guid creditNoteId, Guid invoiceId, decimal amount)
    {
        CreditNoteId = creditNoteId;
        InvoiceId = invoiceId;
        Amount = amount;
    }

    public Guid CreditNoteId { get; }
    public Guid InvoiceId { get; }
    public decimal Amount { get; }
}
