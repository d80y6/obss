using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string TenantId,
    string Name,
    string? Description,
    Guid? CategoryId,
    ProductType ProductType,
    bool IsShippable,
    bool Taxable,
    string? TaxCategory,
    List<ProductSpecificationDto>? Specifications) : IRequest<Result<ProductDto>>;
