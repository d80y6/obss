using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Application.DTOs;

public sealed record CatalogDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    string? CatalogType,
    LifecycleStatus LifecycleStatus,
    int Version,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    DateTime CreatedAt,
    DateTime UpdatedAt);
