using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, string Reason) : IRequest<Result>;
