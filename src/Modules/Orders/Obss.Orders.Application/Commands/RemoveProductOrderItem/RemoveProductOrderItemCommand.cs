using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.RemoveProductOrderItem;

public sealed record RemoveProductOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;
