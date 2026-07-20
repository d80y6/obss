using MediatR;
using Obss.Orders.Application.Commands.Common;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.YemenWiFi;

public sealed record ChangeWifiPackageCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string PackageType,
    int ValidityDays,
    int BandwidthMbps,
    string? Reason) : IRequest<Result<LifecycleResult>>;
