using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Adsl;

public sealed record ChangeAdslProfileCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string LineProfile,
    string? DslPortId,
    int DownstreamSpeedKbps,
    int UpstreamSpeedKbps,
    string? AccessCredentials,
    string? Reason) : IRequest<Result<AdslLifecycleResult>>;
