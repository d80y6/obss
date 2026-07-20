namespace Obss.ServiceQualification.Domain.Services;

public interface IUnifiedQualificationService
{
    Task<UnifiedQualificationResult> QualifyAsync(UnifiedQualificationRequest request, CancellationToken ct);
}

public sealed record UnifiedQualificationRequest(
    string Address,
    string? City,
    string? State,
    double? Latitude,
    double? Longitude,
    string ServiceType,
    string Segment,
    int? SpeedMbps,
    string? TelephoneNumber,
    string? DomainName);

public sealed record UnifiedQualificationResult(
    bool IsQualified,
    string CorrelationId,
    string ServiceType,
    string Explanation,
    string ExplanationAr,
    IReadOnlyList<string>? Alternatives,
    IReadOnlyList<string>? AlternativesAr,
    UnifiedCoverageDetail? CoverageDetail,
    IReadOnlyList<string>? RequiredWork,
    IReadOnlyList<string>? CapacityConflicts);

public sealed record UnifiedCoverageDetail(
    bool TechnologyAvailable,
    bool CapacityAvailable,
    bool PortAvailable,
    string? EstimatedSpeed,
    int? EstimatedInstallationDays,
    string? AdditionalInfo,
    string? AdditionalInfoAr);
