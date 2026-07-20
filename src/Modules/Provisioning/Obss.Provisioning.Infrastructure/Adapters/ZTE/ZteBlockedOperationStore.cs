namespace Obss.Provisioning.Infrastructure.Adapters.ZTE;

public sealed record BlockedOperation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string OperationName { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
    public string RequestPayload { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTime BlockedAt { get; init; } = DateTime.UtcNow;
}

public interface IBlockedOperationStore
{
    Task SaveAsync(BlockedOperation operation, CancellationToken cancellationToken = default);
}
