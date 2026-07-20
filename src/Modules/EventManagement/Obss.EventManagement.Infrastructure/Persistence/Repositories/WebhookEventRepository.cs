using Microsoft.EntityFrameworkCore;
using Obss.EventManagement.Application.Abstractions;
using Obss.EventManagement.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.EventManagement.Infrastructure.Persistence.Repositories;

public sealed class WebhookEventRepository : EfRepository<WebhookEvent>, IWebhookEventRepository
{
    public WebhookEventRepository(EventDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<WebhookEvent>> GetPendingEventsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await DbSet
            .Where(e => e.Status == "pending"
                || (e.Status == "failed" && e.NextAttemptAt != null && e.NextAttemptAt <= now))
            .OrderBy(e => e.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);
    }
}
