using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.CompleteProductOrderItem;

public sealed record CompleteProductOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;
