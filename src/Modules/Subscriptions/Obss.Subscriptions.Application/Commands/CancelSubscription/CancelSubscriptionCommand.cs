using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Commands.CancelSubscription;

public sealed record CancelSubscriptionCommand(
    Guid SubscriptionId,
    string Reason,
    DateTime EffectiveDate) : IRequest<Result>;
