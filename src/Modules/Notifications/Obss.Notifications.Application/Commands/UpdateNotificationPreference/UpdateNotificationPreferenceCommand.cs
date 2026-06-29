using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Commands.UpdateNotificationPreference;

public sealed record UpdateNotificationPreferenceCommand(
    Guid Id,
    Guid CustomerId,
    string NotificationType,
    string Channel,
    bool IsOptedIn) : IRequest<Result<NotificationPreferenceDto>>;
