using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.ApproveProductOrder;

public sealed record ApproveProductOrderCommand(Guid OrderId) : IRequest<Result>;
