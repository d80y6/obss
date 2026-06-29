using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Notifications.Application.Abstractions;
using Obss.Notifications.Application.DTOs;
using Obss.Notifications.Domain.Entities;
using Obss.Notifications.Domain.Exceptions;
using Obss.Notifications.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Notifications.Application.Commands.SendNotificationFromTemplate;

public sealed class SendNotificationFromTemplateCommandHandler
    : IRequestHandler<SendNotificationFromTemplateCommand, Result<NotificationDto>>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IRepository<NotificationTemplate> _templateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendNotificationFromTemplateCommandHandler> _logger;

    public SendNotificationFromTemplateCommandHandler(
        IRepository<Notification> notificationRepository,
        IRepository<NotificationTemplate> templateRepository,
        IUnitOfWork unitOfWork,
        ILogger<SendNotificationFromTemplateCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<NotificationDto>> Handle(
        SendNotificationFromTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);

        if (template is null)
            return Result.Failure<NotificationDto>(
                Error.NotFound(nameof(NotificationTemplate), request.TemplateId));

        if (!template.IsActive)
            return Result.Failure<NotificationDto>(
                Error.Validation($"Template '{template.Name}' is not active."));

        var channel = string.IsNullOrWhiteSpace(request.Channel)
            ? template.NotificationType.ToString()
            : request.Channel;

        (var subject, var body) = template.Render(request.Variables);

        var notificationType = template.NotificationType;
        var priority = NotificationPriority.Normal;

        var notification = Notification.Create(
            request.TenantId,
            notificationType,
            channel,
            request.Recipient,
            subject,
            body,
            priority,
            request.ReferenceType ?? template.Name,
            request.ReferenceId);

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Notification {NotificationId} created from template {TemplateId} for {Recipient}",
            notification.Id, request.TemplateId, request.Recipient);

        return Result.Success(notification.Adapt<NotificationDto>());
    }
}
