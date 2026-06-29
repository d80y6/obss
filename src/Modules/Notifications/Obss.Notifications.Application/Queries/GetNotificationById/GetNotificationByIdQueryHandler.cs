using Mapster;
using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.Notifications.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Queries.GetNotificationById;

public sealed class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDto>>
{
    private readonly IRepository<Notification> _notificationRepository;

    public GetNotificationByIdQueryHandler(IRepository<Notification> notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<NotificationDto>> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification is null)
            return Result.Failure<NotificationDto>(Error.NotFound(nameof(Notification), request.NotificationId));

        return Result.Success(notification.Adapt<NotificationDto>());
    }
}
