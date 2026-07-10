using Obss.SharedKernel.Domain.Common;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Entities;

public class QualificationItem : Entity<Guid>
{
    private readonly List<AlternateServiceProposal> _alternateProposals = [];

    public Guid ServiceId { get; private set; }
    public string ServiceName { get; private set; } = null!;
    public QualificationResultType ResultType { get; private set; }
    public ServiceQualificationItemState State { get; private set; }
    public DateTime? EstimatedInstallDate { get; private set; }
    public DateTime? EstimatedCompletionDate { get; private set; }
    public string? EligibilityUnavailableReason { get; private set; }
    public IReadOnlyCollection<AlternateServiceProposal> AlternateProposals => _alternateProposals.AsReadOnly();

    private QualificationItem() { }

    public QualificationItem(
        Guid id,
        Guid serviceId,
        string serviceName) : base(id)
    {
        ServiceId = serviceId;
        ServiceName = serviceName;
        State = ServiceQualificationItemState.InProgress;
        ResultType = QualificationResultType.UnableToDetermine;
    }

    public void SetResult(
        QualificationResultType resultType,
        DateTime? estimatedInstallDate,
        DateTime? estimatedCompletionDate,
        string? reason)
    {
        ResultType = resultType;
        EstimatedInstallDate = estimatedInstallDate;
        EstimatedCompletionDate = estimatedCompletionDate;
        EligibilityUnavailableReason = reason;
        State = ServiceQualificationItemState.Done;
    }

    public void AddAlternateProposal(AlternateServiceProposal proposal)
    {
        _alternateProposals.Add(proposal);
    }
}
