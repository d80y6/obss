using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceQualification.Domain.Events;

public sealed class ServiceQualificationSubmittedDomainEvent : DomainEvent
{
    public ServiceQualificationSubmittedDomainEvent(Guid qualificationId, Guid customerId)
    {
        QualificationId = qualificationId;
        CustomerId = customerId;
    }

    public Guid QualificationId { get; }
    public Guid CustomerId { get; }
}
