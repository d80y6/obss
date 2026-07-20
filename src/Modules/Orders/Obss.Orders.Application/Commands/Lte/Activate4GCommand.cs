using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Lte;

public sealed record Activate4GCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Iccid,
    string Imsi,
    string Msisdn,
    string ApnName,
    string QosProfile,
    int DataAllowanceMb,
    int ValidityDays) : IRequest<Result<LifecycleResult>>;

public sealed record LifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
