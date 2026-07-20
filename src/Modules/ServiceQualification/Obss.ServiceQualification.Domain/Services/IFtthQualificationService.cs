namespace Obss.ServiceQualification.Domain.Services;

public interface IFtthQualificationService
{
    Task<FtthQualificationResult> QualifyAsync(FtthQualificationRequest request, CancellationToken ct);
}

public sealed record FtthQualificationRequest(
    string Address,
    string? City,
    string? State,
    double? Latitude,
    double? Longitude,
    int RequestedSpeedMbps,
    string Segment);

public sealed record FtthQualificationResult(
    bool IsQualified,
    string CorrelationId,
    string? Explanation,
    string? ExplanationAr,
    IReadOnlyList<string>? Alternatives,
    IReadOnlyList<string>? AlternativesAr,
    FtthCoverageDetail? CoverageDetail,
    IReadOnlyList<string>? RequiredWork,
    IReadOnlyList<string>? CapacityConflicts);

public sealed record FtthCoverageDetail(
    bool FiberAtPremises,
    bool OltCapacityAvailable,
    bool PonPortAvailable,
    bool SplicePointAvailable,
    string? NearestOltName,
    string? EstimatedDistance,
    int? EstimatedInstallationDays,
    bool RequiresAerialWork,
    bool RequiresUndergroundWork);
