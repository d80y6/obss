using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Events;

public sealed class CustomerSuspendedDomainEvent : DomainEvent
{
    public CustomerSuspendedDomainEvent(Guid customerId, string reason)
    {
        CustomerId = customerId;
        Reason = reason;
    }

    public Guid CustomerId { get; }
    public string Reason { get; }
}
