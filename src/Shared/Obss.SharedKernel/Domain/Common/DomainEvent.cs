using MediatR;

namespace Obss.SharedKernel.Domain.Common;

public abstract class DomainEvent : INotification, IEquatable<DomainEvent>
{
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    public Guid EventId { get; }
    public DateTime OccurredOn { get; }

    public bool Equals(DomainEvent? other) => other is not null && EventId == other.EventId;

    public override bool Equals(object? obj) => obj is DomainEvent other && Equals(other);

    public override int GetHashCode() => EventId.GetHashCode();
}
