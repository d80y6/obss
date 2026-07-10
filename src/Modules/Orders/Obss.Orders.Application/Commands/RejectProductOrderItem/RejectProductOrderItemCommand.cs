using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.RejectProductOrderItem;

public sealed record RejectProductOrderItemCommand(Guid OrderId, Guid ItemId, string Reason) : IRequest<Result>;
