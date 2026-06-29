namespace Obss.Audit.Application.DTOs;

public sealed record AuditAlertRuleDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string AlertType,
    string Severity,
    int Threshold,
    int WindowMinutes,
    bool IsActive,
    DateTime CreatedAt);
