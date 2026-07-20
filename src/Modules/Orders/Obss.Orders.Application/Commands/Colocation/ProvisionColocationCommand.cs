using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Colocation;

public sealed record ProvisionColocationCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int RackUnits,
    int PowerWatts,
    int BandwidthMbps,
    int CrossConnects,
    bool RemoteHands,
    bool MonitoringRequired) : IRequest<Result<ColocationLifecycleResult>>;

public sealed record ColocationLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
