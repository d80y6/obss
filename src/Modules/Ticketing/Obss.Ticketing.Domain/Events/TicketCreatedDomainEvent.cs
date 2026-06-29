using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Ticketing.Domain.Events;

public sealed class TicketCreatedDomainEvent : DomainEvent, INotification
{
    public TicketCreatedDomainEvent(Guid ticketId, string ticketNumber, string tenantId, Guid customerId, string subject, string priority)
    {
        TicketId = ticketId;
        TicketNumber = ticketNumber;
        TenantId = tenantId;
        CustomerId = customerId;
        Subject = subject;
        Priority = priority;
    }

    public Guid TicketId { get; }
    public string TicketNumber { get; }
    public string TenantId { get; }
    public Guid CustomerId { get; }
    public string Subject { get; }
    public string Priority { get; }
}
