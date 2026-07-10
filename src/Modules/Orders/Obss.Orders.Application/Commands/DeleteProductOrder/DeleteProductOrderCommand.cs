using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.DeleteProductOrder;

public sealed record DeleteProductOrderCommand(Guid Id) : IRequest<Result>;
