using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.DedicatedServer;

public sealed record ProvisionDedicatedServerCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Hostname,
    int CpuCores,
    int RamGb,
    int StorageGb,
    string StorageType,
    int BandwidthMbps,
    string OperatingSystem,
    string? ControlPanel,
    bool BackupRequired,
    bool MonitoringRequired) : IRequest<Result<DedicatedServerLifecycleResult>>;

public sealed record DedicatedServerLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
