using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    string? TenantId,
    string? Recipient,
    string? NotificationType,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<NotificationDto>>>;
