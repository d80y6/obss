using Obss.SharedKernel.Domain.Common;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Entities;

public class AlternateServiceProposal : Entity<Guid>
{
    public Guid ServiceId { get; private set; }
    public string ServiceName { get; private set; } = null!;
    public QualificationResultType ResultType { get; private set; }
    public DateTime? EstimatedInstallDate { get; private set; }
    public DateTime? GuaranteedUntil { get; private set; }

    private AlternateServiceProposal() { }

    public AlternateServiceProposal(
        Guid id,
        Guid serviceId,
        string serviceName,
        QualificationResultType resultType,
        DateTime? estimatedInstallDate,
        DateTime? guaranteedUntil) : base(id)
    {
        ServiceId = serviceId;
        ServiceName = serviceName;
        ResultType = resultType;
        EstimatedInstallDate = estimatedInstallDate;
        GuaranteedUntil = guaranteedUntil;
    }
}
