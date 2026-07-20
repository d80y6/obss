using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.HomeWireless;

public sealed record ActivateHomeWirelessCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string CpeSerialNumber,
    string CpeImei,
    string Iccid,
    string Imsi,
    string Msisdn,
    string ApnName,
    string QosProfile) : IRequest<Result<HomeWirelessLifecycleResult>>;

public sealed record HomeWirelessLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
