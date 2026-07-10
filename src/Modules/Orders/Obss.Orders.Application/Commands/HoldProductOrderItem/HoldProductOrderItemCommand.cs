using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.HoldProductOrderItem;

public sealed record HoldProductOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;
