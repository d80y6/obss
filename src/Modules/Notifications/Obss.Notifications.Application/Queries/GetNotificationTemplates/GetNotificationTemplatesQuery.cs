using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Queries.GetNotificationTemplates;

public sealed record GetNotificationTemplatesQuery(
    string? TenantId,
    string? NotificationType,
    bool? IsActive) : IRequest<Result<IReadOnlyList<NotificationTemplateDto>>>;
