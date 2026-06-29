using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Events;

public sealed class ProvisioningJobStartedDomainEvent : DomainEvent, INotification
{
    public ProvisioningJobStartedDomainEvent(Guid jobId, Guid orderId, Guid? serviceId, string action)
    {
        JobId = jobId;
        OrderId = orderId;
        ServiceId = serviceId;
        Action = action;
    }

    public Guid JobId { get; }
    public Guid OrderId { get; }
    public Guid? ServiceId { get; }
    public string Action { get; }
}
