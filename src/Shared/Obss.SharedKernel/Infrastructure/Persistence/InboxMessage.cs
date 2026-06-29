namespace Obss.SharedKernel.Infrastructure.Persistence;

public sealed class InboxMessage
{
    private InboxMessage() { }

    public InboxMessage(
        Guid id,
        string eventId,
        string handlerName,
        string eventType,
        string eventData,
        string? tenantId,
        string? correlationId)
    {
        Id = id;
        EventId = eventId;
        HandlerName = handlerName;
        EventType = eventType;
        EventData = eventData;
        ReceivedAt = DateTime.UtcNow;
        TenantId = tenantId;
        CorrelationId = correlationId;
    }

    public Guid Id { get; private set; }
    public string EventId { get; private set; } = string.Empty;
    public string HandlerName { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string EventData { get; private set; } = string.Empty;
    public DateTime ReceivedAt { get; private set; }
    public string? TenantId { get; private set; }
    public string? CorrelationId { get; private set; }
}
