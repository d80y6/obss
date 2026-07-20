using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Pri;

public sealed record ChangePriChannelsCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int NewChannelCount,
    string? Reason) : IRequest<Result<PriLifecycleResult>>;
