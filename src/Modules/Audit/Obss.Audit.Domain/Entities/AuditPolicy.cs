using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Audit.Domain.Entities;

public sealed class AuditPolicy : Entity<Guid>, ITenantEntity
{
    private AuditPolicy() { }

    private AuditPolicy(
        Guid id,
        string tenantId,
        string entityType,
        int retentionDays,
        bool alertOnFailure,
        bool isActive)
        : base(id)
    {
        TenantId = tenantId;
        EntityType = entityType;
        RetentionDays = retentionDays;
        AlertOnFailure = alertOnFailure;
        IsActive = isActive;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public int RetentionDays { get; private set; }
    public bool AlertOnFailure { get; private set; }
    public bool IsActive { get; private set; }

    public static AuditPolicy Create(
        string tenantId,
        string entityType,
        int retentionDays,
        bool alertOnFailure = false,
        bool isActive = true)
    {
        return new AuditPolicy(
            Guid.NewGuid(),
            tenantId,
            entityType,
            retentionDays,
            alertOnFailure,
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

    public void UpdateRetention(int retentionDays)
    {
        RetentionDays = retentionDays;
    }

    public void SetAlertOnFailure(bool alertOnFailure)
    {
        AlertOnFailure = alertOnFailure;
    }
}
