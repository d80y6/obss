using Obss.SharedKernel.Domain.Common;
using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Domain.Entities;

public class QualificationItem : Entity<Guid>
{
    private readonly List<AlternateServiceProposal> _alternateProposals = [];
    private readonly List<string> _alternativeRecommendations = [];
    private readonly List<string> _alternativeRecommendationsAr = [];
    private readonly List<string> _capacityConflicts = [];

    public Guid ServiceId { get; private set; }
    public string ServiceName { get; private set; } = null!;
    public QualificationResultType ResultType { get; private set; }
    public ServiceQualificationItemState State { get; private set; }
    public DateTime? EstimatedInstallDate { get; private set; }
    public DateTime? EstimatedCompletionDate { get; private set; }
    public string? EligibilityUnavailableReason { get; private set; }
    public string? TechnologyType { get; private set; }
    public string? ExplanationAr { get; private set; }
    public int? EstimatedInstallationTimeDays { get; private set; }
    public IReadOnlyCollection<AlternateServiceProposal> AlternateProposals => _alternateProposals.AsReadOnly();
    public IReadOnlyCollection<string> AlternativeRecommendations => _alternativeRecommendations.AsReadOnly();
    public IReadOnlyCollection<string> AlternativeRecommendationsAr => _alternativeRecommendationsAr.AsReadOnly();
    public IReadOnlyCollection<string> CapacityConflicts => _capacityConflicts.AsReadOnly();

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
        string? reason,
        string? technologyType = null,
        string? explanationAr = null,
        int? estimatedInstallationTimeDays = null)
    {
        ResultType = resultType;
        EstimatedInstallDate = estimatedInstallDate;
        EstimatedCompletionDate = estimatedCompletionDate;
        EligibilityUnavailableReason = reason;
        TechnologyType = technologyType;
        ExplanationAr = explanationAr;
        EstimatedInstallationTimeDays = estimatedInstallationTimeDays;
        State = ServiceQualificationItemState.Done;
    }

    public void AddAlternateProposal(AlternateServiceProposal proposal)
    {
        _alternateProposals.Add(proposal);
    }

    public void AddAlternativeRecommendation(string recommendation, string recommendationAr)
    {
        _alternativeRecommendations.Add(recommendation);
        _alternativeRecommendationsAr.Add(recommendationAr);
    }

    public void AddCapacityConflict(string conflict)
    {
        _capacityConflicts.Add(conflict);
    }
}
