using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.ResumeProductOrderItem;

public sealed record ResumeProductOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;
