using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ethernet;

public sealed record TerminateEthernetCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Reason,
    DateTime? RequestedTerminationDate) : IRequest<Result<EthernetLifecycleResult>>;
