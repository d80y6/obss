using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Subscriptions.Application.Commands.RenewSubscription;

public sealed record RenewSubscriptionCommand(Guid SubscriptionId) : IRequest<Result>;
