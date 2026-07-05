using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.DeleteOrder;

public sealed record DeleteOrderCommand(Guid Id) : IRequest<Result>;
