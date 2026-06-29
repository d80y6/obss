using Mapster;
using MediatR;
using Obss.Notifications.Application.Abstractions;
using Obss.Notifications.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Notifications.Application.Queries.GetNotificationTemplates;

public sealed class GetNotificationTemplatesQueryHandler
    : IRequestHandler<GetNotificationTemplatesQuery, Result<IReadOnlyList<NotificationTemplateDto>>>
{
    private readonly INotificationTemplateRepository _templateRepository;

    public GetNotificationTemplatesQueryHandler(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<Result<IReadOnlyList<NotificationTemplateDto>>> Handle(
        GetNotificationTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var templates = await _templateRepository.GetFilteredAsync(
            request.TenantId,
            request.NotificationType,
            request.IsActive,
            cancellationToken);

        var result = templates.Adapt<List<NotificationTemplateDto>>();
        return Result.Success<IReadOnlyList<NotificationTemplateDto>>(result);
    }
}
