using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceInventory.Domain.Events;

public sealed class ServiceActivatedDomainEvent : DomainEvent
{
    public ServiceActivatedDomainEvent(Guid serviceId, Guid tenantId, Guid customerId)
    {
        ServiceId = serviceId;
        TenantId = tenantId;
        CustomerId = customerId;
    }

    public Guid ServiceId { get; }
    public Guid TenantId { get; }
    public Guid CustomerId { get; }
}
