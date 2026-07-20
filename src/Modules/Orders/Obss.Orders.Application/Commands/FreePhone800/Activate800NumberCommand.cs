using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.FreePhone800;

public sealed record Activate800NumberCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string FreePhoneNumber,
    string CustomerName,
    string CustomerNameAr,
    List<string> TerminatingNumbers,
    bool BusinessHoursOnly,
    int MaxMonthlyMinutes,
    string ChargingParty) : IRequest<Result<FreePhone800LifecycleResult>>;

public sealed record FreePhone800LifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
