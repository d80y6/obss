using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Tdm;

public sealed record TerminateTdmCircuitCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Reason,
    DateTime? RequestedTerminationDate) : IRequest<Result<TdmLifecycleResult>>;
