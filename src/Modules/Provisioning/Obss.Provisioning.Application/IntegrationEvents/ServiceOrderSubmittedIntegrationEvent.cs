using Obss.SharedKernel.Domain.Events;

namespace Obss.Provisioning.Application.IntegrationEvents;

public sealed class ServiceOrderSubmittedIntegrationEvent : IntegrationEvent
{
    public ServiceOrderSubmittedIntegrationEvent(
        Guid serviceOrderId,
        Guid tenantId,
        string? externalId)
    {
        ServiceOrderId = serviceOrderId;
        TenantId = tenantId.ToString();
        ExternalId = externalId;
    }

    public Guid ServiceOrderId { get; }
    public string? ExternalId { get; }
}
