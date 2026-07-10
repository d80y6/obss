using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceQualification.Domain.Events;

public sealed class ServiceQualificationCompletedDomainEvent : DomainEvent
{
    public ServiceQualificationCompletedDomainEvent(Guid qualificationId, Guid customerId, bool isFullyQualified)
    {
        QualificationId = qualificationId;
        CustomerId = customerId;
        IsFullyQualified = isFullyQualified;
    }

    public Guid QualificationId { get; }
    public Guid CustomerId { get; }
    public bool IsFullyQualified { get; }
}
