using MediatR;
using Obss.Audit.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Audit.Domain.Events;

public sealed class AuditAlertTriggeredDomainEvent : DomainEvent, INotification
{
    public AuditAlertTriggeredDomainEvent(
        Guid alertId,
        string tenantId,
        AlertType alertType,
        AlertSeverity severity,
        string message,
        string? entityType,
        string? entityId)
    {
        AlertId = alertId;
        TenantId = tenantId;
        AlertType = alertType;
        Severity = severity;
        Message = message;
        EntityType = entityType;
        EntityId = entityId;
    }

    public Guid AlertId { get; }
    public string TenantId { get; }
    public AlertType AlertType { get; }
    public AlertSeverity Severity { get; }
    public string Message { get; }
    public string? EntityType { get; }
    public string? EntityId { get; }
}
