namespace Obss.Notifications.Application.DTOs;

public sealed record NotificationPreferenceDto(
    Guid Id,
    Guid CustomerId,
    string NotificationType,
    string Channel,
    bool IsOptedIn,
    DateTime? OptInAt,
    DateTime? OptOutAt);
