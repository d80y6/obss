using Obss.SharedKernel.Domain.Common;

namespace Obss.ServiceQualification.Domain.Events;

public sealed class ServiceQualificationTerminatedDomainEvent : DomainEvent
{
    public ServiceQualificationTerminatedDomainEvent(Guid qualificationId, string reason)
    {
        QualificationId = qualificationId;
        Reason = reason;
    }

    public Guid QualificationId { get; }
    public string Reason { get; }
}
