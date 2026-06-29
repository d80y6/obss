namespace Obss.Notifications.Application.DTOs;

public sealed record NotificationTemplateDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string NotificationType,
    string Subject,
    string Body,
    List<string> Variables,
    bool IsActive,
    int Version,
    DateTime CreatedAt,
    DateTime UpdatedAt);
