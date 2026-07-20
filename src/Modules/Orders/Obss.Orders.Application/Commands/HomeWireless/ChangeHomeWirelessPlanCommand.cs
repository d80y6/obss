using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.HomeWireless;

public sealed record ChangeHomeWirelessPlanCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string ApnName,
    string QosProfile,
    int? DataAllowanceMb,
    int? ValidityDays,
    string? Reason) : IRequest<Result<HomeWirelessLifecycleResult>>;
