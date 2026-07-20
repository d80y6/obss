namespace Obss.ServiceQualification.Domain.Services;

public interface ITelephonyQualificationService
{
    Task<TelephonyQualificationResult> QualifyAsync(TelephonyQualificationRequest request, CancellationToken ct);
}

public sealed record TelephonyQualificationRequest(
    string Address,
    string? City,
    string? State,
    string TelephoneNumber,
    string Segment);

public sealed record TelephonyQualificationResult(
    bool IsQualified,
    string CorrelationId,
    string? Explanation,
    string? ExplanationAr,
    IReadOnlyList<string>? Alternatives,
    IReadOnlyList<string>? AlternativesAr,
    TelephonyCoverageDetail? CoverageDetail,
    IReadOnlyList<string>? RequiredWork,
    IReadOnlyList<string>? CapacityConflicts);

public sealed record TelephonyCoverageDetail(
    bool NumberAvailable,
    bool SoftswitchCapacityAvailable,
    int? EstimatedInstallationDays,
    string? AdditionalInfo,
    string? AdditionalInfoAr);
