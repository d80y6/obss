using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Events;

public sealed class InvoiceDisputeResolvedDomainEvent : DomainEvent, INotification
{
    public InvoiceDisputeResolvedDomainEvent(Guid disputeId, Guid invoiceId, string resolution)
    {
        DisputeId = disputeId;
        InvoiceId = invoiceId;
        Resolution = resolution;
    }

    public Guid DisputeId { get; }
    public Guid InvoiceId { get; }
    public string Resolution { get; }
}
