using Obss.Notifications.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Notifications.Application.Abstractions;

public interface INotificationTemplateRepository : IRepository<NotificationTemplate>
{
    Task<IReadOnlyList<NotificationTemplate>> GetFilteredAsync(
        string? tenantId,
        string? notificationType,
        bool? isActive,
        CancellationToken cancellationToken = default);
}
