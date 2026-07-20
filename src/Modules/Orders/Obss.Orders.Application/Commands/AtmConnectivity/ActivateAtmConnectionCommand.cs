using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.AtmConnectivity;

public sealed record ActivateAtmConnectionCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string TerminalId,
    string TerminalLocation,
    string TerminalType,
    string ConnectivityType,
    int BandwidthMbps,
    int VlanId,
    string HostEndpoint,
    string? SecurityRequirements) : IRequest<Result<AtmLifecycleResult>>;

public sealed record AtmLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
