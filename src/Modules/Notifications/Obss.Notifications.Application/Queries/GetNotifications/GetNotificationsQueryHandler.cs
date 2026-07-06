using Mapster;
using MediatR;
using Obss.Notifications.Application.Abstractions;
using Obss.Notifications.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetFilteredAsync(
            request.TenantId,
            request.Recipient,
            request.NotificationType,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Offset,
            request.Limit,
            cancellationToken);

        var result = notifications.Adapt<List<NotificationDto>>();
        return Result.Success<IReadOnlyList<NotificationDto>>(result);
    }
}
