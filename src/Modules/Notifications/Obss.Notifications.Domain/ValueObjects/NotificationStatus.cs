namespace Obss.Notifications.Domain.ValueObjects;

public enum NotificationStatus
{
    Pending,
    Sent,
    Delivered,
    Failed,
    Bounced,
    Read
}
