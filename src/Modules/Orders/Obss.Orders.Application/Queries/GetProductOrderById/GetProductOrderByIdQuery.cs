using MediatR;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetProductOrderById;

public sealed record GetProductOrderByIdQuery(Guid OrderId) : IRequest<Result<ProductOrderDto>>;
