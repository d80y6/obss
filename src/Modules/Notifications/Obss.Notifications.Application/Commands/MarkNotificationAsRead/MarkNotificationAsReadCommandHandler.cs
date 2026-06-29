using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Notifications.Application.Abstractions;
using Obss.Notifications.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Commands.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkNotificationAsReadCommandHandler> _logger;

    public MarkNotificationAsReadCommandHandler(
        IRepository<Notification> notificationRepository,
        IUnitOfWork unitOfWork,
        ILogger<MarkNotificationAsReadCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification is null)
            return Result.Failure(Error.NotFound("Notification", request.NotificationId));

        notification.MarkAsRead();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Notification {NotificationId} marked as read",
            notification.Id);

        return Result.Success();
    }
}
