namespace Obss.IAM.Application.DTOs;

public sealed record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    string? ConnectionString,
    bool IsActive,
    string? Settings,
    DateTime CreatedAt,
    DateTime UpdatedAt);
