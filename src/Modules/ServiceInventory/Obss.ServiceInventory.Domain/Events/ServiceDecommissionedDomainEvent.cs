using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceInventory.Domain.Events;

public sealed class ServiceDecommissionedDomainEvent : DomainEvent
{
    public ServiceDecommissionedDomainEvent(Guid serviceId, Guid customerId)
    {
        ServiceId = serviceId;
        CustomerId = customerId;
    }

    public Guid ServiceId { get; }
    public Guid CustomerId { get; }
}
