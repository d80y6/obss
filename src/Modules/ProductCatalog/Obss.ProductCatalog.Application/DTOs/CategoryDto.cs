namespace Obss.ProductCatalog.Application.DTOs;

public sealed record CategoryDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    bool IsActive,
    int SortOrder,
    DateTime CreatedAt);
