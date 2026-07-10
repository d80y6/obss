using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.StartProductOrderItem;

public sealed record StartProductOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;
