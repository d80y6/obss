using Obss.Notifications.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Notifications.Application.Abstractions;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetFilteredAsync(
        string? tenantId,
        string? recipient,
        string? notificationType,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default);
}
