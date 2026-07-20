using MediatR;
using Obss.Orders.Application.Commands.Common;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.YemenWiFi;

public sealed record ActivateWifiAccessCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string SiteName,
    string SiteNameAr,
    string AccessPointId,
    string PackageType,
    int ValidityDays,
    int BandwidthMbps,
    string Username,
    string Password,
    bool SplashPageEnabled) : IRequest<Result<LifecycleResult>>;
