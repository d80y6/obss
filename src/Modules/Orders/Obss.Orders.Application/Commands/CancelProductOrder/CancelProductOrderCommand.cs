using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.CancelProductOrder;

public sealed record CancelProductOrderCommand(Guid OrderId, string Reason) : IRequest<Result>;
