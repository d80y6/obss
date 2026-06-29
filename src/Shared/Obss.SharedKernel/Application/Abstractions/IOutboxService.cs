namespace Obss.SharedKernel.Application.Abstractions;

public interface IOutboxService
{
    Task AddAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class;

    Task ProcessPendingAsync(CancellationToken cancellationToken = default);
}

public interface IInboxService
{
    Task<bool> IsProcessedAsync(string eventId, string handlerName, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string eventId, string handlerName, CancellationToken cancellationToken = default);
}