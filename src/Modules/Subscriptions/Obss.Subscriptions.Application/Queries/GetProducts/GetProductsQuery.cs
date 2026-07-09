using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProducts;

public sealed record GetProductsQuery : IRequest<Result<List<ProductDto>>>;
