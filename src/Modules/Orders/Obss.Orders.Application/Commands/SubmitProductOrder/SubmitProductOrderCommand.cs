using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SubmitProductOrder;

public sealed record SubmitProductOrderCommand(Guid OrderId) : IRequest<Result>;
