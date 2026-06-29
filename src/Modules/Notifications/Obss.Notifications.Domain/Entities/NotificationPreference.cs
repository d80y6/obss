using Obss.Notifications.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Notifications.Domain.Entities;

public sealed class NotificationPreference : Entity<Guid>
{
    private NotificationPreference() { }

    public NotificationPreference(
        Guid id,
        Guid customerId,
        NotificationType notificationType,
        string channel)
        : base(id)
    {
        CustomerId = customerId;
        NotificationType = notificationType;
        Channel = channel;
        IsOptedIn = true;
        OptInAt = DateTime.UtcNow;
    }

    public Guid CustomerId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public string Channel { get; private set; } = string.Empty;
    public bool IsOptedIn { get; private set; }
    public DateTime? OptInAt { get; private set; }
    public DateTime? OptOutAt { get; private set; }

    public void OptIn()
    {
        if (IsOptedIn)
            return;

        IsOptedIn = true;
        OptInAt = DateTime.UtcNow;
        OptOutAt = null;
    }

    public void OptOut()
    {
        if (!IsOptedIn)
            return;

        IsOptedIn = false;
        OptOutAt = DateTime.UtcNow;
    }
}
