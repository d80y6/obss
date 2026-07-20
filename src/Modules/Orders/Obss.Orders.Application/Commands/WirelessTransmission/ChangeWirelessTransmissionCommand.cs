using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.WirelessTransmission;

public sealed record ChangeWirelessTransmissionCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int? NewBandwidthMbps,
    string? NewAntennaType,
    string? Reason) : IRequest<Result<WirelessTransmissionLifecycleResult>>;
