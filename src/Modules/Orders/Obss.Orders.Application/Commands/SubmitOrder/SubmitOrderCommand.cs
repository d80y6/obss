using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SubmitOrder;

public sealed record SubmitOrderCommand(Guid OrderId) : IRequest<Result>;
