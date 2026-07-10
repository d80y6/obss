using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.AssessProductOrderItem;

public sealed record AssessProductOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;
