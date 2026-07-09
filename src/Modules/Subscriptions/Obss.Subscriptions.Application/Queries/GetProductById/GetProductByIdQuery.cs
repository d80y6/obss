using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;
