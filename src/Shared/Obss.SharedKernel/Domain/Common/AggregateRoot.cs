using Obss.SharedKernel.Domain.Events;

namespace Obss.SharedKernel.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IntegrationEvent> _integrationEvents = [];

    protected AggregateRoot(TId id) : base(id) { }

    protected AggregateRoot() { }

    public IReadOnlyCollection<IntegrationEvent> IntegrationEvents => _integrationEvents.AsReadOnly();

    protected void AddIntegrationEvent(IntegrationEvent integrationEvent) => _integrationEvents.Add(integrationEvent);

    protected void RemoveIntegrationEvent(IntegrationEvent integrationEvent) => _integrationEvents.Remove(integrationEvent);

    public void ClearIntegrationEvents() => _integrationEvents.Clear();
}