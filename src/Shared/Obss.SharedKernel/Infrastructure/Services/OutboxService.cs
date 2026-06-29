using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.SharedKernel.Infrastructure.Services;

public sealed class OutboxService : IOutboxService
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task AddAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class
    {
        var eventType = integrationEvent.GetType().Name;
        var eventData = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType());

        var outboxMessage = new OutboxMessage(
            Guid.NewGuid(),
            eventType,
            eventData,
            tenantId: null,
            correlationId: null);

        var dbContexts = _serviceProvider.GetServices<EfDbContext>();
        var dbContext = dbContexts.First();
        await dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
    }

    public async Task ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var dbContexts = _serviceProvider.GetServices<EfDbContext>();
        foreach (var ctx in dbContexts)
        {
            var pending = await ctx.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .Take(100)
                .ToListAsync(cancellationToken);

            foreach (var message in pending)
            {
                message.MarkAsProcessed();
            }

            await ctx.SaveChangesAsync(cancellationToken);
        }
    }
}
