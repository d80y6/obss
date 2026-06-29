using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Events;

public sealed class InvoicePaidDomainEvent : DomainEvent, INotification
{
    public InvoicePaidDomainEvent(Guid invoiceId, string invoiceNumber, decimal amountPaid, string paymentReference)
    {
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        AmountPaid = amountPaid;
        PaymentReference = paymentReference;
    }

    public Guid InvoiceId { get; }
    public string InvoiceNumber { get; }
    public decimal AmountPaid { get; }
    public string PaymentReference { get; }
}
