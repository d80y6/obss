using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Ticketing.Domain.Events;

public sealed class TicketAssignedDomainEvent : DomainEvent, INotification
{
    public TicketAssignedDomainEvent(Guid ticketId, string ticketNumber, string assignedTo, string assignedBy)
    {
        TicketId = ticketId;
        TicketNumber = ticketNumber;
        AssignedTo = assignedTo;
        AssignedBy = assignedBy;
    }

    public Guid TicketId { get; }
    public string TicketNumber { get; }
    public string AssignedTo { get; }
    public string AssignedBy { get; }
}
