using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Vps;

public sealed record ChangeVpsResourcesCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int? NewCpuCores,
    int? NewRamMb,
    int? NewStorageGb,
    int? NewBandwidthMbps,
    string? Reason) : IRequest<Result<VpsLifecycleResult>>;
