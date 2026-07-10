using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.FailProductOrderItem;

public sealed record FailProductOrderItemCommand(Guid OrderId, Guid ItemId, string Error) : IRequest<Result>;
