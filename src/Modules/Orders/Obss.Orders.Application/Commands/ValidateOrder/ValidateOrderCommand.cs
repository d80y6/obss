using MediatR;
using Obss.Orders.Application.Services;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.ValidateOrder;

public sealed record ValidateOrderCommand(Guid OrderId) : IRequest<Result<OrderValidationResult>>;
