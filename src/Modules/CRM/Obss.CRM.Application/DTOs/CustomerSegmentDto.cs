namespace Obss.CRM.Application.DTOs;

public sealed record CustomerSegmentDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string Criteria,
    int Priority,
    bool IsActive,
    int CustomerCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
