using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Pri;

public sealed record ActivatePriTrunkCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int ChannelCount,
    string SignalingProtocol,
    string TrunkGroup,
    string Endpoint,
    string RoutingNumber) : IRequest<Result<PriLifecycleResult>>;
