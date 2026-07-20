namespace Obss.ServiceQualification.Domain.Services;

public interface IWifiQualificationService
{
    Task<WifiQualificationResult> QualifyAsync(WifiQualificationRequest request, CancellationToken ct);
}

public sealed record WifiQualificationRequest(
    string Address,
    string? City,
    string? State,
    double? Latitude,
    double? Longitude,
    string Segment);

public sealed record WifiQualificationResult(
    bool IsQualified,
    string CorrelationId,
    string? Explanation,
    string? ExplanationAr,
    IReadOnlyList<string>? Alternatives,
    IReadOnlyList<string>? AlternativesAr,
    WifiCoverageDetail? CoverageDetail,
    IReadOnlyList<string>? RequiredWork,
    IReadOnlyList<string>? CapacityConflicts);

public sealed record WifiCoverageDetail(
    bool HotspotAvailable,
    bool AccessPointCapacityAvailable,
    int? EstimatedInstallationDays,
    string? AdditionalInfo,
    string? AdditionalInfoAr);
