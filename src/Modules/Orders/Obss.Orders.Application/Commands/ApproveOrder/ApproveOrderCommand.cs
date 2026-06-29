using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.ApproveOrder;

public sealed record ApproveOrderCommand(Guid OrderId) : IRequest<Result>;
