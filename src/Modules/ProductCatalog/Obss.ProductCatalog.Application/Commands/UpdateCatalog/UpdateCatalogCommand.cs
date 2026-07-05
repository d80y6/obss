using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateCatalog;

public sealed record UpdateCatalogCommand(
    Guid CatalogId,
    string Name,
    string? Description,
    string? CatalogType,
    int Version,
    DateTime? ValidFrom,
    DateTime? ValidTo) : IRequest<Result<CatalogDto>>;
