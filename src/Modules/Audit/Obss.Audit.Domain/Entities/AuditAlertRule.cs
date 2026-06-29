using Obss.Audit.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Audit.Domain.Entities;

public sealed class AuditAlertRule : AggregateRoot<Guid>, ITenantEntity
{
    private AuditAlertRule() { }

    private AuditAlertRule(
        Guid id,
        string tenantId,
        string name,
        string? description,
        AlertType alertType,
        AlertSeverity severity,
        int threshold,
        int windowMinutes,
        bool isActive)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        AlertType = alertType;
        Severity = severity;
        Threshold = threshold;
        WindowMinutes = windowMinutes;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AlertType AlertType { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public int Threshold { get; private set; }
    public int WindowMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static AuditAlertRule Create(
        string tenantId,
        string name,
        string? description,
        AlertType alertType,
        AlertSeverity severity,
        int threshold,
        int windowMinutes,
        bool isActive = true)
    {
        return new AuditAlertRule(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            alertType,
            severity,
            threshold,
            windowMinutes,
            isActive);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
