using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Adsl;

public sealed record SuspendAdslCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Reason) : IRequest<Result<AdslLifecycleResult>>;

public sealed record AdslLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
