using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Events;

public sealed class InvoiceOverdueDomainEvent : DomainEvent, INotification
{
    public InvoiceOverdueDomainEvent(Guid invoiceId, string invoiceNumber, int daysOverdue)
    {
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        DaysOverdue = daysOverdue;
    }

    public Guid InvoiceId { get; }
    public string InvoiceNumber { get; }
    public int DaysOverdue { get; }
}
