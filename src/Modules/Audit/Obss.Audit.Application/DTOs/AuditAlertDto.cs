namespace Obss.Audit.Application.DTOs;

public sealed record AuditAlertDto(
    Guid Id,
    string TenantId,
    string Severity,
    string AlertType,
    string Message,
    string? EntityType,
    string? EntityId,
    DateTime DetectedAt,
    bool IsAcknowledged,
    string? AcknowledgedById,
    DateTime? AcknowledgedAt,
    DateTime? ResolvedAt,
    DateTime CreatedAt);
