using MediatR;
using Obss.ProductCatalog.Application.Contracts;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProducts;

public sealed record GetProductsQuery(
    Guid? CategoryId,
    ProductType? ProductType,
    LifecycleStatus? Status,
    string? SearchTerm,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<PaginatedResult<ProductDto>>>;
