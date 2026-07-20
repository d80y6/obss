using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Dia;

public sealed record SuspendDiaCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Reason) : IRequest<Result<DiaLifecycleResult>>;

public sealed record DiaLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
