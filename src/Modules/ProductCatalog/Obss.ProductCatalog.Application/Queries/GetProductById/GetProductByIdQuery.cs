using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductDto>>;
