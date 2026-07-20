using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.DedicatedServer;

public sealed record ChangeDedicatedServerCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int? NewCpuCores,
    int? NewRamGb,
    int? NewStorageGb,
    string? NewStorageType,
    int? NewBandwidthMbps,
    string? Reason) : IRequest<Result<DedicatedServerLifecycleResult>>;
