using Obss.ApiGateway.Domain.ValueObjects;

namespace Obss.ApiGateway.Application.DTOs;

public sealed record ApiKeyDto(
    Guid Id,
    string TenantId,
    Guid? PartnerId,
    string Name,
    string Key,
    ApiKeyStatus Status,
    List<string> Permissions,
    List<string> AllowedIPs,
    int RateLimitPerMinute,
    DateTime? ExpiresAt,
    DateTime CreatedAt,
    DateTime? RevokedAt);
