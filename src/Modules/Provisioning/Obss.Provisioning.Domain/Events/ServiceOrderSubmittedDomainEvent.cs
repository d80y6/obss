using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Events;

public sealed class ServiceOrderSubmittedDomainEvent : DomainEvent, INotification
{
    public ServiceOrderSubmittedDomainEvent(Guid serviceOrderId, Guid tenantId, string? externalId)
    {
        ServiceOrderId = serviceOrderId;
        TenantId = tenantId;
        ExternalId = externalId;
    }

    public Guid ServiceOrderId { get; }
    public Guid TenantId { get; }
    public string? ExternalId { get; }
}
