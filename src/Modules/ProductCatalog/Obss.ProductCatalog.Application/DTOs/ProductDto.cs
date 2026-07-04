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
    string? ProductNumber,
    Guid? ProductSpecificationId,
    LifecycleStatus LifecycleStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ProductSpecValueDto> Specifications,
    List<OfferDto> Offers);

public sealed record ProductSpecValueDto(
    string Name,
    string Value,
    bool IsRequired);
