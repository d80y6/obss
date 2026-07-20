namespace Obss.Rating.Application.Services;

public sealed record RawCdrRecord(
    string CorrelationId,
    string Vendor,
    string Payload,
    string SourceIp,
    DateTime ReceivedAt);

public sealed record CdrValidationResult(
    bool IsValid,
    string? ErrorReason);

public sealed record CdrIngestResult(
    int Accepted,
    int Duplicates,
    int Quarantined,
    IReadOnlyCollection<string> Errors);

public sealed record CdrReplayResult(
    int Replayed,
    int StillInvalid,
    IReadOnlyCollection<string> Errors);

public sealed record NormalizedCdrData(
    string SubscriberId,
    string SessionId,
    long BytesUp,
    long BytesDown,
    long Duration,
    DateTime StartTime,
    DateTime EndTime,
    string? Apn,
    string? Qos,
    string? CallingNumber,
    string? CalledNumber,
    string? CallType,
    string? TrunkId,
    string? BillingParty);

public interface ICdrMediationService
{
    Task<CdrIngestResult> IngestBatchAsync(IReadOnlyCollection<RawCdrRecord> records, CancellationToken cancellationToken = default);
    Task<CdrReplayResult> ReplayQuarantinedAsync(CancellationToken cancellationToken = default);
}
