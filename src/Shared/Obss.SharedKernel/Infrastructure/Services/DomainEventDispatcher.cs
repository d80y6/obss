using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.Common;

namespace Obss.SharedKernel.Infrastructure.Services;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAsync(IReadOnlyCollection<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    public async Task DispatchAndClearAsync(Entity<Guid> entity, CancellationToken cancellationToken = default)
    {
        var domainEvents = entity.DomainEvents.ToList();
        entity.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
