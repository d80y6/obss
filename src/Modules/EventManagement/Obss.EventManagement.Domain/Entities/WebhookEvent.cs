using Obss.SharedKernel.Domain.Common;

namespace Obss.EventManagement.Domain.Entities;

public class WebhookEvent : Entity<Guid>
{
    private WebhookEvent() { }

    public WebhookEvent(
        Guid id,
        Guid subscriptionId,
        string eventType,
        string payload,
        string status,
        int retryCount = 0)
        : base(id)
    {
        SubscriptionId = subscriptionId;
        EventType = eventType;
        Payload = payload;
        Status = status;
        RetryCount = retryCount;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid SubscriptionId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string Status { get; private set; } = "pending";
    public int RetryCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? LastError { get; private set; }

    public void MarkDelivered()
    {
        Status = "delivered";
        DeliveredAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = "failed";
        LastError = error;
        RetryCount++;
    }
}
