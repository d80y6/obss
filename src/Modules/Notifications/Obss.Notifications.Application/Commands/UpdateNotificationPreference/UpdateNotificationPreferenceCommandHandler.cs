using Mapster;
using MediatR;
using Obss.Notifications.Application.DTOs;
using Obss.Notifications.Domain.Entities;
using Obss.Notifications.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Commands.UpdateNotificationPreference;

public sealed class UpdateNotificationPreferenceCommandHandler
    : IRequestHandler<UpdateNotificationPreferenceCommand, Result<NotificationPreferenceDto>>
{
    private readonly IRepository<NotificationPreference> _preferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNotificationPreferenceCommandHandler(
        IRepository<NotificationPreference> preferenceRepository,
        IUnitOfWork unitOfWork)
    {
        _preferenceRepository = preferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NotificationPreferenceDto>> Handle(
        UpdateNotificationPreferenceCommand request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<NotificationType>(request.NotificationType, true, out var notificationType))
            return Result.Failure<NotificationPreferenceDto>(
                Error.Validation($"Invalid notification type: '{request.NotificationType}'."));

        var existing = await _preferenceRepository.GetByIdAsync(request.Id, cancellationToken);

        if (existing is null)
        {
            var preference = new NotificationPreference(
                request.Id,
                request.CustomerId,
                notificationType,
                request.Channel);

            if (!request.IsOptedIn)
                preference.OptOut();

            await _preferenceRepository.AddAsync(preference, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(preference.Adapt<NotificationPreferenceDto>());
        }

        if (request.IsOptedIn)
            existing.OptIn();
        else
            existing.OptOut();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(existing.Adapt<NotificationPreferenceDto>());
    }
}
