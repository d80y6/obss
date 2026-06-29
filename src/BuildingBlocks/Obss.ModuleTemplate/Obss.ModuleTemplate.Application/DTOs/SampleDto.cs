namespace Obss.ModuleTemplate.Application.DTOs;

public sealed record SampleDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
