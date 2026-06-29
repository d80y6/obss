using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Events;

public sealed class CustomerStatusChangedDomainEvent : DomainEvent
{
    public CustomerStatusChangedDomainEvent(
        Guid customerId,
        CustomerStatus oldStatus,
        CustomerStatus newStatus)
    {
        CustomerId = customerId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }

    public Guid CustomerId { get; }
    public CustomerStatus OldStatus { get; }
    public CustomerStatus NewStatus { get; }
}
