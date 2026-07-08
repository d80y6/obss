using Obss.SharedKernel.Domain.Events;

namespace Obss.Provisioning.Application.IntegrationEvents;

public sealed class ServiceOrderItemCompletedIntegrationEvent : IntegrationEvent
{
    public ServiceOrderItemCompletedIntegrationEvent(
        Guid serviceOrderId,
        Guid itemId,
        Guid? serviceId,
        string? externalId)
    {
        ServiceOrderId = serviceOrderId;
        ItemId = itemId;
        ServiceId = serviceId;
        ExternalId = externalId;
    }

    public Guid ServiceOrderId { get; }
    public Guid ItemId { get; }
    public Guid? ServiceId { get; }
    public string? ExternalId { get; }
}
