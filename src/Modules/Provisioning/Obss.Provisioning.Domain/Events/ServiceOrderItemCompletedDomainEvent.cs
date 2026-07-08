using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Events;

public sealed class ServiceOrderItemCompletedDomainEvent : DomainEvent, INotification
{
    public ServiceOrderItemCompletedDomainEvent(Guid serviceOrderId, Guid itemId, Guid? serviceId)
    {
        ServiceOrderId = serviceOrderId;
        ItemId = itemId;
        ServiceId = serviceId;
    }

    public Guid ServiceOrderId { get; }
    public Guid ItemId { get; }
    public Guid? ServiceId { get; }
}
