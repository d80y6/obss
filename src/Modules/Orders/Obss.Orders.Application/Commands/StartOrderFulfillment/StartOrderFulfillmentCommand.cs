using MediatR;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.StartOrderFulfillment;

public sealed record StartOrderFulfillmentCommand(Guid OrderId) : IRequest<Result<OrderFulfillmentDto>>;
