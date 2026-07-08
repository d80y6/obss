using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Events;

public sealed class ServiceOrderCancellationRequestedDomainEvent : DomainEvent, INotification
{
    public ServiceOrderCancellationRequestedDomainEvent(Guid serviceOrderId, string? externalId, string reason)
    {
        ServiceOrderId = serviceOrderId;
        ExternalId = externalId;
        Reason = reason;
    }

    public Guid ServiceOrderId { get; }
    public string? ExternalId { get; }
    public string Reason { get; }
}
