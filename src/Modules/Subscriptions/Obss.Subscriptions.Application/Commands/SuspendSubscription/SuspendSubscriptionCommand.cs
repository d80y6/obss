using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Commands.SuspendSubscription;

public sealed record SuspendSubscriptionCommand(
    Guid SubscriptionId,
    string Reason) : IRequest<Result>;
