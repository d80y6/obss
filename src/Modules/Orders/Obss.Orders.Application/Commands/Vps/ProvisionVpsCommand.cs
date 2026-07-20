using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Vps;

public sealed record ProvisionVpsCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Hostname,
    int CpuCores,
    int RamMb,
    int StorageGb,
    int BandwidthMbps,
    string OperatingSystem,
    string VirtualizationType,
    bool BackupRequired,
    bool SnapshotRequired,
    int PublicIpCount) : IRequest<Result<VpsLifecycleResult>>;

public sealed record VpsLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
