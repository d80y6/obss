using MediatR;

namespace Obss.SharedKernel.Domain.Events;

public abstract class IntegrationEvent : INotification
{
    protected IntegrationEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public string EventType => GetType().Name;
    public string TenantId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}