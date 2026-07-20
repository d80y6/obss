using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.DedicatedServer;

public sealed record SuspendDedicatedServerCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string Reason) : IRequest<Result<DedicatedServerLifecycleResult>>;
