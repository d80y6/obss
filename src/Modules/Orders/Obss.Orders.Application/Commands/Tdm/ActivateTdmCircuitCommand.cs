using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Tdm;

public sealed record ActivateTdmCircuitCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string CircuitType,
    string EndpointA,
    string EndpointB,
    int ChannelCount,
    string Framing,
    string LineCoding) : IRequest<Result<TdmLifecycleResult>>;
