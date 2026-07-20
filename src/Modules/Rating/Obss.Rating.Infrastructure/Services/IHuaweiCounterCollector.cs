namespace Obss.Rating.Infrastructure.Services;

public sealed record HuaweiCounterData(
    string SubscriberId,
    string SessionId,
    long BytesTransmitted,
    long BytesReceived,
    long SessionDuration,
    DateTime Timestamp,
    bool IsActive);

public sealed record CounterCollectionResult(
    IReadOnlyCollection<HuaweiCounterData> Counters,
    IReadOnlyCollection<string> Errors);

public interface IHuaweiCounterCollector
{
    Task<CounterCollectionResult> CollectCountersAsync(
        string deviceAddress,
        string community,
        int port = 161,
        CancellationToken cancellationToken = default);

    Task<CounterCollectionResult> CollectViaRestconfAsync(
        string deviceAddress,
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
