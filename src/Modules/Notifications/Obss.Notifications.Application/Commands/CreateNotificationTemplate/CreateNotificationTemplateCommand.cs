using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Commands.CreateNotificationTemplate;

public sealed record CreateNotificationTemplateCommand(
    string TenantId,
    string Name,
    string? Description,
    string NotificationType,
    string Subject,
    string Body,
    List<string>? Variables) : IRequest<Result<NotificationTemplateDto>>;
