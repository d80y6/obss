using System.Collections.Concurrent;

namespace Obss.SharedKernel.Domain.Common;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected Entity(TId id)
    {
        Id = id;
    }

    protected Entity() { }

    public TId Id { get; protected init; } = default!;

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public override bool Equals(object? obj) => obj is Entity<TId> other && Id.Equals(other.Id);

    public bool Equals(Entity<TId>? other) => other is not null && Id.Equals(other.Id);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !(left == right);

    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    protected void RemoveDomainEvent(DomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
