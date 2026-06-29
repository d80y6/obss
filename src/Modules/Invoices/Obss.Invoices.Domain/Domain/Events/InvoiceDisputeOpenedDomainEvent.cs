using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Events;

public sealed class InvoiceDisputeOpenedDomainEvent : DomainEvent, INotification
{
    public InvoiceDisputeOpenedDomainEvent(Guid disputeId, Guid invoiceId, Guid customerId, decimal disputedAmount, string reason)
    {
        DisputeId = disputeId;
        InvoiceId = invoiceId;
        CustomerId = customerId;
        DisputedAmount = disputedAmount;
        Reason = reason;
    }

    public Guid DisputeId { get; }
    public Guid InvoiceId { get; }
    public Guid CustomerId { get; }
    public decimal DisputedAmount { get; }
    public string Reason { get; }
}
