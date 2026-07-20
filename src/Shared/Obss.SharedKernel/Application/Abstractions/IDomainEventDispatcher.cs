using Obss.SharedKernel.Domain.Common;

namespace Obss.SharedKernel.Application.Abstractions;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
    Task DispatchAndClearAsync(Entity<Guid> entity, CancellationToken cancellationToken = default);
}
