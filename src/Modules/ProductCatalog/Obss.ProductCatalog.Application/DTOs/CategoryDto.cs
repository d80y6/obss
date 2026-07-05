using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Application.DTOs;

public sealed record CategoryDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    bool IsActive,
    LifecycleStatus LifecycleStatus,
    int SortOrder,
    int Version,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    DateTime CreatedAt,
    DateTime UpdatedAt);
