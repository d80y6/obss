using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Dia;

public sealed record ResumeDiaCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string? Reason) : IRequest<Result<DiaLifecycleResult>>;
