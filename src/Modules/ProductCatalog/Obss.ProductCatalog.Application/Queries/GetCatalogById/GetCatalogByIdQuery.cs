using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetCatalogById;

public sealed record GetCatalogByIdQuery(Guid CatalogId) : IRequest<Result<CatalogDto>>;
