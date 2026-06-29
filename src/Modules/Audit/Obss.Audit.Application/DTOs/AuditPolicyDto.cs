namespace Obss.Audit.Application.DTOs;

public sealed record AuditPolicyDto(
    Guid Id,
    string TenantId,
    string EntityType,
    int RetentionDays,
    bool AlertOnFailure,
    bool IsActive);
