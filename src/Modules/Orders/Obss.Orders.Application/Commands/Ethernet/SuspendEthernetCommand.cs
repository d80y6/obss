using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ethernet;

public sealed record SuspendEthernetCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Reason) : IRequest<Result<EthernetLifecycleResult>>;

public sealed record EthernetLifecycleResult(
    Guid CorrelationId,
    string Status,
    string Message,
    string MessageAr,
    Guid? ProvisioningJobId);
