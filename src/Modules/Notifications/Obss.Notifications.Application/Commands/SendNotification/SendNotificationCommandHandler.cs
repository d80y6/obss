using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Notifications.Application.Abstractions;
using Obss.Notifications.Application.DTOs;
using Obss.Notifications.Domain.Entities;
using Obss.Notifications.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Commands.SendNotification;

public sealed class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Result<NotificationDto>>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendNotificationCommandHandler> _logger;

    public SendNotificationCommandHandler(
        IRepository<Notification> notificationRepository,
        IUnitOfWork unitOfWork,
        ILogger<SendNotificationCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<NotificationDto>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<NotificationType>(request.NotificationType, true, out var notificationType))
            return Result.Failure<NotificationDto>(Error.Validation($"Invalid notification type: '{request.NotificationType}'."));

        if (!Enum.TryParse<NotificationPriority>(request.Priority ?? "Normal", true, out var priority))
            return Result.Failure<NotificationDto>(Error.Validation($"Invalid priority: '{request.Priority}'."));

        var notification = Notification.Create(
            request.TenantId,
            notificationType,
            request.Channel,
            request.Recipient,
            request.Subject,
            request.Body,
            priority,
            request.ReferenceType,
            request.ReferenceId);

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Notification {NotificationId} created for {Recipient} via {Channel}",
            notification.Id, request.Recipient, request.Channel);

        return Result.Success(notification.Adapt<NotificationDto>());
    }
}
