using Mapster;
using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.Notifications.Domain.Entities;
using Obss.Notifications.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Commands.CreateNotificationTemplate;

public sealed class CreateNotificationTemplateCommandHandler
    : IRequestHandler<CreateNotificationTemplateCommand, Result<NotificationTemplateDto>>
{
    private readonly IRepository<NotificationTemplate> _templateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNotificationTemplateCommandHandler(
        IRepository<NotificationTemplate> templateRepository,
        IUnitOfWork unitOfWork)
    {
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NotificationTemplateDto>> Handle(
        CreateNotificationTemplateCommand request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<NotificationType>(request.NotificationType, true, out var notificationType))
            return Result.Failure<NotificationTemplateDto>(
                Error.Validation($"Invalid notification type: '{request.NotificationType}'."));

        var template = NotificationTemplate.Create(
            request.TenantId,
            request.Name,
            request.Description,
            notificationType,
            request.Subject,
            request.Body,
            request.Variables);

        await _templateRepository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(template.Adapt<NotificationTemplateDto>());
    }
}
