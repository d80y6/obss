using MediatR;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetOrderFulfillmentStatus;

public sealed record GetOrderFulfillmentStatusQuery(Guid OrderId) : IRequest<Result<OrderFulfillmentDto>>;
