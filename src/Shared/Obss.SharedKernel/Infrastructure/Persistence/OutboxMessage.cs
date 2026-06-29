namespace Obss.SharedKernel.Infrastructure.Persistence;

public sealed class OutboxMessage
{
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
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string EventData { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? TenantId { get; private set; }
    public string? CorrelationId { get; private set; }

    public void MarkAsProcessed() => ProcessedAt = DateTime.UtcNow;
}
