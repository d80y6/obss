namespace Obss.ServiceQualification.Domain.Services;

public interface IBusinessServiceQualificationService
{
    Task<BusinessServiceQualificationResult> QualifyAsync(BusinessServiceQualificationRequest request, CancellationToken ct);
}

public sealed record BusinessServiceQualificationRequest(
    string Address,
    string? City,
    string? State,
    double? Latitude,
    double? Longitude,
    string BusinessServiceType,
    int? BandwidthMbps,
    string? DomainName,
    string Segment);

public sealed record BusinessServiceQualificationResult(
    bool IsQualified,
    string CorrelationId,
    string? Explanation,
    string? ExplanationAr,
    IReadOnlyList<string>? Alternatives,
    IReadOnlyList<string>? AlternativesAr,
    BusinessCoverageDetail? CoverageDetail,
    IReadOnlyList<string>? RequiredWork,
    IReadOnlyList<string>? CapacityConflicts);

public sealed record BusinessCoverageDetail(
    bool FiberAvailable,
    bool CopperAvailable,
    bool CapacityAvailable,
    bool DataCenterSpaceAvailable,
    bool DomainNameAvailable,
    int? EstimatedInstallationDays,
    string? AdditionalInfo,
    string? AdditionalInfoAr);
