using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceInventory.Domain.Events;

public sealed class ServiceSuspendedDomainEvent : DomainEvent
{
    public ServiceSuspendedDomainEvent(Guid serviceId, string reason)
    {
        ServiceId = serviceId;
        Reason = reason;
    }

    public Guid ServiceId { get; }
    public string Reason { get; }
}
