using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.SharedKernel.Infrastructure.Services;

public sealed class InboxService : IInboxService
{
    private readonly IEnumerable<EfDbContext> _contexts;

    public InboxService(IEnumerable<EfDbContext> contexts)
    {
        _contexts = contexts;
    }

    public async Task<bool> IsProcessedAsync(
        string eventId,
        string handlerName,
        CancellationToken cancellationToken = default)
    {
        foreach (var context in _contexts)
        {
            var exists = await context.InboxMessages
                .AnyAsync(m => m.EventId == eventId && m.HandlerName == handlerName, cancellationToken);

            if (exists)
                return true;
        }

        return false;
    }

    public async Task MarkAsProcessedAsync(
        string eventId,
        string handlerName,
        CancellationToken cancellationToken = default)
    {
        foreach (var context in _contexts)
        {
            var exists = await context.InboxMessages
                .AnyAsync(m => m.EventId == eventId && m.HandlerName == handlerName, cancellationToken);

            if (!exists)
            {
                context.InboxMessages.Add(new InboxMessage(
                    Guid.NewGuid(),
                    eventId,
                    handlerName,
                    string.Empty,
                    string.Empty,
                    null,
                    null));
                await context.SaveChangesAsync(cancellationToken);
                return;
            }
        }
    }
}
