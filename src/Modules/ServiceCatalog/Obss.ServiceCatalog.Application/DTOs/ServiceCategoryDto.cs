namespace Obss.ServiceCatalog.Application.DTOs;

public sealed record ServiceCategoryDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string LifecycleStatus,
    int Version,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsRoot
);
