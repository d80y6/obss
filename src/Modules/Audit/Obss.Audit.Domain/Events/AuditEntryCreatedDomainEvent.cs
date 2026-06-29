using MediatR;
using Obss.Audit.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Audit.Domain.Events;

public sealed class AuditEntryCreatedDomainEvent : DomainEvent, INotification
{
    public AuditEntryCreatedDomainEvent(
        Guid auditEntryId,
        string tenantId,
        string entityType,
        string entityId,
        AuditAction action,
        string? performedById,
        bool isSensitive)
    {
        AuditEntryId = auditEntryId;
        TenantId = tenantId;
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        PerformedById = performedById;
        IsSensitive = isSensitive;
    }

    public Guid AuditEntryId { get; }
    public string TenantId { get; }
    public string EntityType { get; }
    public string EntityId { get; }
    public AuditAction Action { get; }
    public string? PerformedById { get; }
    public bool IsSensitive { get; }
}
