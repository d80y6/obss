using Obss.SharedKernel.Domain.Events;

namespace Obss.Provisioning.Application.IntegrationEvents;

public sealed class ServiceOrderStateChangedIntegrationEvent : IntegrationEvent
{
    public ServiceOrderStateChangedIntegrationEvent(
        Guid serviceOrderId,
        string? externalId,
        string newState)
    {
        ServiceOrderId = serviceOrderId;
        ExternalId = externalId;
        NewState = newState;
    }

    public Guid ServiceOrderId { get; }
    public string? ExternalId { get; }
    public string NewState { get; }
}
