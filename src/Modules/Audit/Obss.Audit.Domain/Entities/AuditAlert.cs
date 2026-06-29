using Obss.Audit.Domain.Events;
using Obss.Audit.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Audit.Domain.Entities;

public sealed class AuditAlert : AggregateRoot<Guid>, ITenantEntity
{
    private AuditAlert() { }

    private AuditAlert(
        Guid id,
        string tenantId,
        AlertSeverity severity,
        AlertType alertType,
        string message,
        string? entityType,
        string? entityId)
        : base(id)
    {
        TenantId = tenantId;
        Severity = severity;
        AlertType = alertType;
        Message = message;
        EntityType = entityType;
        EntityId = entityId;
        DetectedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        IsAcknowledged = false;

        AddDomainEvent(new AuditAlertTriggeredDomainEvent(
            id, tenantId, alertType, severity, message, entityType, entityId));
    }

    public string TenantId { get; private set; } = string.Empty;
    public AlertSeverity Severity { get; private set; }
    public AlertType AlertType { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string? EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public DateTime DetectedAt { get; private set; }
    public bool IsAcknowledged { get; private set; }
    public string? AcknowledgedById { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static AuditAlert Create(
        string tenantId,
        AlertSeverity severity,
        AlertType alertType,
        string message,
        string? entityType = null,
        string? entityId = null)
    {
        return new AuditAlert(
            Guid.NewGuid(),
            tenantId,
            severity,
            alertType,
            message,
            entityType,
            entityId);
    }

    public void Acknowledge(string userId)
    {
        IsAcknowledged = true;
        AcknowledgedById = userId;
        AcknowledgedAt = DateTime.UtcNow;
    }

    public void Resolve()
    {
        ResolvedAt = DateTime.UtcNow;
    }
}
