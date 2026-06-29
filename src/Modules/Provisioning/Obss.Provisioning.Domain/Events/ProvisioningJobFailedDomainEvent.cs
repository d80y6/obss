using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Events;

public sealed class ProvisioningJobFailedDomainEvent : DomainEvent, INotification
{
    public ProvisioningJobFailedDomainEvent(Guid jobId, string errorMessage)
    {
        JobId = jobId;
        ErrorMessage = errorMessage;
    }

    public Guid JobId { get; }
    public string ErrorMessage { get; }
}
