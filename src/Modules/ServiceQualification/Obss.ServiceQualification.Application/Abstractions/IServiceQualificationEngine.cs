using Obss.ServiceQualification.Domain.ValueObjects;

namespace Obss.ServiceQualification.Application.Abstractions;

public record QualificationRequestItem(Guid ServiceId, string ServiceName);

public record QualificationEngineItemResult(
    Guid ServiceId,
    string ServiceName,
    QualificationResultType ResultType,
    DateTime? EstimatedInstallDate,
    DateTime? EstimatedCompletionDate,
    string? Reason,
    List<AlternateProposalResult> AlternateProposals);

public record AlternateProposalResult(
    Guid ServiceId,
    string ServiceName,
    QualificationResultType ResultType,
    DateTime? EstimatedInstallDate,
    DateTime? GuaranteedUntil);

public record QualificationEngineResult(List<QualificationEngineItemResult> Items);

public interface IServiceQualificationEngine
{
    Task<QualificationEngineResult> QualifyAsync(
        GeographicAddress address,
        IReadOnlyList<QualificationRequestItem> requestedServices,
        CancellationToken cancellationToken);
}
