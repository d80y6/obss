namespace Obss.ServiceQualification.Domain.Services;

public interface IAdslQualificationService
{
    Task<AdslQualificationResult> QualifyAsync(AdslQualificationRequest request, CancellationToken ct);
}

public sealed record AdslQualificationRequest(
    string Address,
    string? City,
    string? State,
    double? Latitude,
    double? Longitude,
    int RequestedSpeedMbps,
    string Segment);

public sealed record AdslQualificationResult(
    bool IsQualified,
    string CorrelationId,
    string? Explanation,
    string? ExplanationAr,
    IReadOnlyList<string>? Alternatives,
    IReadOnlyList<string>? AlternativesAr,
    AdslCoverageDetail? CoverageDetail,
    IReadOnlyList<string>? RequiredWork,
    IReadOnlyList<string>? CapacityConflicts);

public sealed record AdslCoverageDetail(
    bool CopperLineAvailable,
    bool DslamPortAvailable,
    bool DslamCapacityAvailable,
    string? DistanceFromExchange,
    string? EstimatedSpeed,
    int? EstimatedInstallationDays,
    string? AdditionalInfo,
    string? AdditionalInfoAr);
