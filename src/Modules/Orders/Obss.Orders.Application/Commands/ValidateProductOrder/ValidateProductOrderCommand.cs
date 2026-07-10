using MediatR;
using Obss.Orders.Application.Services;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.ValidateProductOrder;

public sealed record ValidateProductOrderCommand(Guid OrderId) : IRequest<Result<OrderValidationResult>>;
