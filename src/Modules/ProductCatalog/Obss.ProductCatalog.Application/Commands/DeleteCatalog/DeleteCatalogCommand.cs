using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.DeleteCatalog;

public sealed record DeleteCatalogCommand(Guid CatalogId) : IRequest<Result>;
