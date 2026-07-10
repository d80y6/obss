using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.AcknowledgeProductOrderItem;

public sealed record AcknowledgeProductOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;
