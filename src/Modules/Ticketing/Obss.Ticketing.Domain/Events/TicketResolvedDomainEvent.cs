using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Ticketing.Domain.Events;

public sealed class TicketResolvedDomainEvent : DomainEvent, INotification
{
    public TicketResolvedDomainEvent(Guid ticketId, string ticketNumber, string resolution, string resolvedBy)
    {
        TicketId = ticketId;
        TicketNumber = ticketNumber;
        Resolution = resolution;
        ResolvedBy = resolvedBy;
    }

    public Guid TicketId { get; }
    public string TicketNumber { get; }
    public string Resolution { get; }
    public string ResolvedBy { get; }
}
