using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateCatalog;

public sealed record CreateCatalogCommand(
    string TenantId,
    string Name,
    string? Description,
    string? CatalogType,
    int Version,
    DateTime? ValidFrom,
    DateTime? ValidTo) : IRequest<Result<CatalogDto>>;
