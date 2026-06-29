namespace Obss.Audit.Application.DTOs;

public sealed record AuditEntryDto(
    Guid Id,
    string TenantId,
    string EntityType,
    string EntityId,
    string Action,
    string? Changes,
    string? PerformedById,
    string? PerformedByName,
    DateTime PerformedAt,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId,
    bool IsSensitive);
