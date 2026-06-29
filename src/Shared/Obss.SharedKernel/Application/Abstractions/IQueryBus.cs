using MediatR;

namespace Obss.SharedKernel.Application.Abstractions;

public interface IQueryBus
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> query, CancellationToken cancellationToken = default);
}

public interface ICommandBus
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> command, CancellationToken cancellationToken = default);
    Task SendAsync(IRequest command, CancellationToken cancellationToken = default);
}

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : INotification;
}