using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Commands.SendNotification;

public sealed record SendNotificationCommand(
    string TenantId,
    string NotificationType,
    string Channel,
    string Recipient,
    string Subject,
    string Body,
    string? Priority,
    string? ReferenceType,
    Guid? ReferenceId) : IRequest<Result<NotificationDto>>;
