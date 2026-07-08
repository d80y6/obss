using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Events;

public sealed class ServiceOrderStateChangedDomainEvent : DomainEvent, INotification
{
    public ServiceOrderStateChangedDomainEvent(Guid serviceOrderId, string? externalId, string newState)
    {
        ServiceOrderId = serviceOrderId;
        ExternalId = externalId;
        NewState = newState;
    }

    public Guid ServiceOrderId { get; }
    public string? ExternalId { get; }
    public string NewState { get; }
}
