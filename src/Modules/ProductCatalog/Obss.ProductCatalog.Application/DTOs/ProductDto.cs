using Obss.ProductCatalog.Domain.Domain.ValueObjects;

namespace Obss.ProductCatalog.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    string TenantId,
    string Name,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    ProductType ProductType,
    bool IsActive,
    bool IsShippable,
    bool Taxable,
    string? TaxCategory,
    LifecycleStatus LifecycleStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ProductSpecificationDto> Specifications,
    List<OfferDto> Offers);

public sealed record ProductSpecificationDto(
    string Name,
    string Value,
    bool IsRequired);
