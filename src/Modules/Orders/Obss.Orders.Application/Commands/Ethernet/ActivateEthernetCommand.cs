using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ethernet;

public sealed record ActivateEthernetCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int BandwidthMbps,
    int VlanId,
    string Encapsulation,
    string EndpointA,
    string EndpointB,
    bool RedundancyRequired) : IRequest<Result<EthernetLifecycleResult>>;
