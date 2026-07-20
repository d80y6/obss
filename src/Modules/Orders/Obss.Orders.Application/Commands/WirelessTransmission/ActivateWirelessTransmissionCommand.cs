using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.WirelessTransmission;

public sealed record ActivateWirelessTransmissionCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int BandwidthMbps,
    string EndpointA,
    string EndpointB,
    string AntennaType,
    string FrequencyBand,
    double RangeKm,
    string SlaLevel) : IRequest<Result<WirelessTransmissionLifecycleResult>>;
