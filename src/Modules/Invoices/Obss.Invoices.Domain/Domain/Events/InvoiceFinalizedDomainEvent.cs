using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Events;

public sealed class InvoiceFinalizedDomainEvent : DomainEvent, INotification
{
    public InvoiceFinalizedDomainEvent(Guid invoiceId, string invoiceNumber, Guid customerId, decimal grandTotal)
    {
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        CustomerId = customerId;
        GrandTotal = grandTotal;
    }

    public Guid InvoiceId { get; }
    public string InvoiceNumber { get; }
    public Guid CustomerId { get; }
    public decimal GrandTotal { get; }
}
