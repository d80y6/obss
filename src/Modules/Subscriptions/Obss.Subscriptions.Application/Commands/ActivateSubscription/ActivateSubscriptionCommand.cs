using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Commands.ActivateSubscription;

public sealed record ActivateSubscriptionCommand(Guid SubscriptionId) : IRequest<Result>;
