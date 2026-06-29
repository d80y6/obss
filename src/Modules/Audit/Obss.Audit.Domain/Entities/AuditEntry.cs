using Obss.Audit.Domain.Events;
using Obss.Audit.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Audit.Domain.Entities;

public sealed class AuditEntry : AggregateRoot<Guid>, ITenantEntity
{
    private AuditEntry() { }

    private AuditEntry(
        Guid id,
        string tenantId,
        string entityType,
        string entityId,
        AuditAction action,
        string? changes,
        string? performedById,
        string? performedByName,
        DateTime performedAt,
        string? ipAddress,
        string? userAgent,
        string? correlationId,
        bool isSensitive)
        : base(id)
    {
        TenantId = tenantId;
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        Changes = changes;
        PerformedById = performedById;
        PerformedByName = performedByName;
        PerformedAt = performedAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CorrelationId = correlationId;
        IsSensitive = isSensitive;

        AddDomainEvent(new AuditEntryCreatedDomainEvent(
            id, tenantId, entityType, entityId, action, performedById, isSensitive));
    }

    public string TenantId { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public AuditAction Action { get; private set; }
    public string? Changes { get; private set; }
    public string? PerformedById { get; private set; }
    public string? PerformedByName { get; private set; }
    public DateTime PerformedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? CorrelationId { get; private set; }
    public bool IsSensitive { get; private set; }

    public static AuditEntry Create(
        string tenantId,
        string entityType,
        string entityId,
        AuditAction action,
        string? changes = null,
        string? performedById = null,
        string? performedByName = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null,
        bool isSensitive = false)
    {
        return new AuditEntry(
            Guid.NewGuid(),
            tenantId,
            entityType,
            entityId,
            action,
            changes,
            performedById,
            performedByName,
            DateTime.UtcNow,
            ipAddress,
            userAgent,
            correlationId,
            isSensitive);
    }
}
