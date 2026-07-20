namespace Obss.ServiceQualification.Domain.Services;

public interface ILteQualificationService
{
    Task<LteQualificationResult> QualifyAsync(LteQualificationRequest request, CancellationToken ct);
}

public sealed record LteQualificationRequest(
    string Address,
    string? City,
    string? State,
    double? Latitude,
    double? Longitude,
    int RequestedSpeedMbps,
    string Segment);

public sealed record LteQualificationResult(
    bool IsQualified,
    string CorrelationId,
    string? Explanation,
    string? ExplanationAr,
    IReadOnlyList<string>? Alternatives,
    IReadOnlyList<string>? AlternativesAr,
    LteCoverageDetail? CoverageDetail,
    IReadOnlyList<string>? RequiredWork,
    IReadOnlyList<string>? CapacityConflicts);

public sealed record LteCoverageDetail(
    bool CoverageAvailable,
    bool CellCapacityAvailable,
    string? EstimatedSignalStrength,
    string? EstimatedSpeed,
    int? EstimatedInstallationDays,
    string? AdditionalInfo,
    string? AdditionalInfoAr);
