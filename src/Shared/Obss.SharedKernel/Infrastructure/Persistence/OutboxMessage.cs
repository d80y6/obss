namespace Obss.SharedKernel.Infrastructure.Persistence;

public sealed class OutboxMessage
{
    private const int _maxRetryCount = 5;

    private OutboxMessage() { }

    public OutboxMessage(
        Guid id,
        string eventType,
        string eventData,
        string? tenantId,
        string? correlationId)
    {
        Id = id;
        EventType = eventType;
        EventData = eventData;
        CreatedAt = DateTime.UtcNow;
        TenantId = tenantId;
        CorrelationId = correlationId;
        RetryCount = 0;
        IsDeadLettered = false;
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string EventData { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? TenantId { get; private set; }
    public string? CorrelationId { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTime? NextAttemptAt { get; private set; }
    public bool IsDeadLettered { get; private set; }
    public Guid? LockId { get; private set; }
    public DateTime? LockExpiresAt { get; private set; }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        LockId = null;
        LockExpiresAt = null;
    }

    public bool TryAcquireLock(Guid lockId, TimeSpan lockDuration)
    {
        if (LockExpiresAt.HasValue && LockExpiresAt > DateTime.UtcNow && LockId != lockId)
            return false;

        LockId = lockId;
        LockExpiresAt = DateTime.UtcNow.Add(lockDuration);
        return true;
    }

    public void ReleaseLock()
    {
        LockId = null;
        LockExpiresAt = null;
    }

    public bool CanRetry()
    {
        return RetryCount < _maxRetryCount && !IsDeadLettered && ProcessedAt is null;
    }

    public void RecordFailure(string error)
    {
        RetryCount++;
        LastError = error;
        NextAttemptAt = DateTime.UtcNow.AddSeconds(CalculateBackoff());
    }

    public void MarkAsDeadLettered(string error)
    {
        IsDeadLettered = true;
        LastError = error;
        ProcessedAt = DateTime.UtcNow;
        LockId = null;
        LockExpiresAt = null;
    }

    private double CalculateBackoff()
    {
        return Math.Min(Math.Pow(2, RetryCount) * 10, 300);
    }
}
