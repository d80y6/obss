namespace Obss.AAA.Application.DTOs;

public sealed record AaaAuditLogDto(
    Guid Id,
    string TenantId,
    string EventType,
    string? Username,
    Guid? NasId,
    string? NasIpAddress,
    string? Detail,
    DateTime Timestamp);
