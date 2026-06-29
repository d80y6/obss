using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Events;

public sealed class ProvisioningJobCompletedDomainEvent : DomainEvent, INotification
{
    public ProvisioningJobCompletedDomainEvent(Guid jobId, Guid? serviceId)
    {
        JobId = jobId;
        ServiceId = serviceId;
    }

    public Guid JobId { get; }
    public Guid? ServiceId { get; }
}
