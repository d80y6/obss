using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.RemoveOrderItem;

public sealed record RemoveOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;
