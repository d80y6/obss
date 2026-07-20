using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Dia;

public sealed record ChangeDiaBandwidthCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int NewBandwidthMbps,
    string? Reason) : IRequest<Result<DiaLifecycleResult>>;
