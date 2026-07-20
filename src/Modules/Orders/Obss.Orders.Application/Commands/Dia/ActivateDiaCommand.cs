using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Dia;

public sealed record ActivateDiaCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int BandwidthMbps,
    string InterfaceType,
    string HandoffLocation,
    int StaticIpCount,
    int? BurstBandwidthMbps,
    string SlaLevel) : IRequest<Result<DiaLifecycleResult>>;
