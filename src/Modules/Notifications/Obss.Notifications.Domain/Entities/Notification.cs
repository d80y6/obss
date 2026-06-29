using Obss.Notifications.Domain.Events;
using Obss.Notifications.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Notifications.Domain.Entities;

public class Notification : AggregateRoot<Guid>
{
    private Notification() { }

    private Notification(
        Guid id,
        string tenantId,
        NotificationType notificationType,
        string channel,
        string recipient,
        string subject,
        string body,
        NotificationPriority priority,
        string? referenceType,
        Guid? referenceId)
        : base(id)
    {
        TenantId = tenantId;
        NotificationType = notificationType;
        Channel = channel;
        Recipient = recipient;
        Subject = subject;
        Body = body;
        Status = NotificationStatus.Pending;
        Priority = priority;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public NotificationType NotificationType { get; private set; }
    public string Channel { get; private set; } = string.Empty;
    public string Recipient { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Notification Create(
        string tenantId,
        NotificationType notificationType,
        string channel,
        string recipient,
        string subject,
        string body,
        NotificationPriority priority = NotificationPriority.Normal,
        string? referenceType = null,
        Guid? referenceId = null)
    {
        return new Notification(
            Guid.NewGuid(),
            tenantId,
            notificationType,
            channel,
            recipient,
            subject,
            body,
            priority,
            referenceType,
            referenceId);
    }

    public void MarkSent()
    {
        if (Status != NotificationStatus.Pending)
            return;

        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationSentDomainEvent(
            Id, Recipient, Channel, ReferenceType, ReferenceId));
    }

    public void MarkDelivered()
    {
        if (Status != NotificationStatus.Sent)
            return;

        Status = NotificationStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        if (Status is NotificationStatus.Delivered or NotificationStatus.Bounced)
            return;

        Status = NotificationStatus.Failed;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = error;

        AddDomainEvent(new NotificationFailedDomainEvent(
            Id, Recipient, Channel, error, ReferenceType, ReferenceId));
    }

    public void MarkBounced(string reason)
    {
        Status = NotificationStatus.Bounced;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = reason;
    }

    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Read)
            return;

        Status = NotificationStatus.Read;
        ReadAt = DateTime.UtcNow;
    }
}
