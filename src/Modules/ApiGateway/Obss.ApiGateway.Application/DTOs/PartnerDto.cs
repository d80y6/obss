using Obss.ApiGateway.Domain.ValueObjects;

namespace Obss.ApiGateway.Application.DTOs;

public sealed record PartnerDto(
    Guid Id,
    string TenantId,
    string Name,
    string ContactName,
    string ContactEmail,
    List<string> AllowedIPs,
    bool IsActive,
    SlaLevel SlaLevel,
    int MaxRequestsPerDay,
    DateTime CreatedAt,
    List<ApiKeyDto>? ApiKeys);
