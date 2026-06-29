using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Ticketing.Domain.Events;

public sealed class TicketEscalatedDomainEvent : DomainEvent, INotification
{
    public TicketEscalatedDomainEvent(Guid ticketId, string ticketNumber, string escalatedBy, string reason)
    {
        TicketId = ticketId;
        TicketNumber = ticketNumber;
        EscalatedBy = escalatedBy;
        Reason = reason;
    }

    public Guid TicketId { get; }
    public string TicketNumber { get; }
    public string EscalatedBy { get; }
    public string Reason { get; }
}
