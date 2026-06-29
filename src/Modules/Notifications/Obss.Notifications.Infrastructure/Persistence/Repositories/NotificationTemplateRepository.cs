using Microsoft.EntityFrameworkCore;
using Obss.Notifications.Application.Abstractions;
using Obss.Notifications.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Notifications.Infrastructure.Persistence.Repositories;

public sealed class NotificationTemplateRepository
    : EfRepository<NotificationTemplate>, INotificationTemplateRepository
{
    public NotificationTemplateRepository(NotificationDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<NotificationTemplate>> GetFilteredAsync(
        string? tenantId,
        string? notificationType,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenantId))
            query = query.Where(t => t.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(notificationType))
            query = query.Where(t => t.NotificationType.ToString() == notificationType);

        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        query = query.OrderByDescending(t => t.Version);

        return await query.ToListAsync(cancellationToken);
    }
}
