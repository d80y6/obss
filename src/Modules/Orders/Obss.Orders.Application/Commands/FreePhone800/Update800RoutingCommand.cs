using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.FreePhone800;

public sealed record Update800RoutingCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string FreePhoneNumber,
    List<string> TerminatingNumbers,
    bool BusinessHoursOnly,
    int MaxMonthlyMinutes,
    string ChargingParty) : IRequest<Result<FreePhone800LifecycleResult>>;
