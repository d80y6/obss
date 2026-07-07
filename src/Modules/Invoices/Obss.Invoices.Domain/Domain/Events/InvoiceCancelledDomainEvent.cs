using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Events;

public sealed class InvoiceCancelledDomainEvent : DomainEvent, INotification
{
    public InvoiceCancelledDomainEvent(Guid invoiceId, string invoiceNumber, string reason)
    {
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        Reason = reason;
    }

    public Guid InvoiceId { get; }
    public string InvoiceNumber { get; }
    public string Reason { get; }
}
