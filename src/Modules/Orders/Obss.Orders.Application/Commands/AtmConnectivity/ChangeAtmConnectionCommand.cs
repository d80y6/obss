using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.AtmConnectivity;

public sealed record ChangeAtmConnectionCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int? NewBandwidthMbps,
    int? NewVlanId,
    string? NewHostEndpoint,
    string? NewSecurityRequirements,
    string? Reason) : IRequest<Result<AtmLifecycleResult>>;
