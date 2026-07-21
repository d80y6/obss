namespace Obss.AAA.Application.DTOs;

public sealed record NasDto(
    Guid Id,
    string TenantId,
    string Name,
    string NasIpAddress,
    string NasType,
    string Status,
    string? Location,
    DateTime CreatedAt,
    DateTime UpdatedAt);
