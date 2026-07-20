using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ethernet;

public sealed record ChangeEthernetBandwidthCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int NewBandwidthMbps,
    string? Reason) : IRequest<Result<EthernetLifecycleResult>>;
