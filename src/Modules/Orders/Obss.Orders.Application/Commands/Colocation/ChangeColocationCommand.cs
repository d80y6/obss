using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Colocation;

public sealed record ChangeColocationCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int? NewRackUnits,
    int? NewPowerWatts,
    int? NewBandwidthMbps,
    int? NewCrossConnects,
    string? Reason) : IRequest<Result<ColocationLifecycleResult>>;
