using Obss.SharedKernel.Domain.Common;

namespace Obss.EventManagement.Domain.Entities;

public class WebhookEvent : AggregateRoot<Guid>
{
    private const int _maxRetryCount = 5;

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
    public DateTime? NextAttemptAt { get; private set; }

    public void MarkDelivered()
    {
        Status = "delivered";
        DeliveredAt = DateTime.UtcNow;
        NextAttemptAt = null;
    }

    public void MarkFailed(string error)
    {
        RetryCount++;
        LastError = error;

        if (RetryCount >= _maxRetryCount)
        {
            Status = "dead_letter";
            NextAttemptAt = null;
        }
        else
        {
            Status = "failed";
            NextAttemptAt = DateTime.UtcNow.AddSeconds(CalculateBackoff());
        }
    }

    private double CalculateBackoff()
    {
        return Math.Min(Math.Pow(2, RetryCount) * 10, 300);
    }
}
