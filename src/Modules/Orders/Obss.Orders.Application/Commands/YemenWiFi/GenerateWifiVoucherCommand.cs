using MediatR;
using Obss.Orders.Application.Commands.Common;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.YemenWiFi;

public sealed record GenerateWifiVoucherCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string AccessPointId,
    int VoucherCount,
    int ValidityHours,
    int BandwidthMbps,
    int DataAllowanceMb) : IRequest<Result<LifecycleResult>>;
