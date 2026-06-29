using Microsoft.EntityFrameworkCore;
using Obss.Notifications.Application.Abstractions;
using Obss.Notifications.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Notifications.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository : EfRepository<Notification>, INotificationRepository
{
    public NotificationRepository(NotificationDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Notification>> GetFilteredAsync(
        string? tenantId,
        string? recipient,
        string? notificationType,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
            query = query.Where(n => n.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(recipient))
            query = query.Where(n => n.Recipient.Contains(recipient));

        if (!string.IsNullOrWhiteSpace(notificationType))
            query = query.Where(n => n.NotificationType.ToString() == notificationType);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(n => n.Status.ToString() == status);

        if (fromDate.HasValue)
            query = query.Where(n => n.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(n => n.CreatedAt <= toDate.Value);

        query = query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(n => n.Status == Domain.ValueObjects.NotificationStatus.Pending)
            .OrderBy(n => n.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}
