using MediatR;
using Obss.ProductCatalog.Application.Contracts;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetCatalogs;

public sealed record GetCatalogsQuery(
    string? SearchTerm,
    string? CatalogType,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedResult<CatalogDto>>>;
