namespace Obss.Notifications.Application.DTOs;

public sealed record NotificationDto(
    Guid Id,
    string TenantId,
    string NotificationType,
    string Channel,
    string Recipient,
    string Subject,
    string Body,
    string Status,
    string Priority,
    DateTime? SentAt,
    DateTime? DeliveredAt,
    DateTime? FailedAt,
    string? ErrorMessage,
    string? ReferenceType,
    Guid? ReferenceId,
    DateTime CreatedAt);
