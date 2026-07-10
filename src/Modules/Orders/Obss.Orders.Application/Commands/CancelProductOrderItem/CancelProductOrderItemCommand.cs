using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.CancelProductOrderItem;

public sealed record CancelProductOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;
