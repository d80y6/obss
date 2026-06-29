using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Domain.Common;
using Obss.Subscriptions.Domain.Events;

namespace Obss.Subscriptions.Application.DomainEventHandlers;

public sealed class SubscriptionActivatedEventHandler
{
    private readonly ILogger<SubscriptionActivatedEventHandler> _logger;

    public SubscriptionActivatedEventHandler(ILogger<SubscriptionActivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SubscriptionActivatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Subscription {SubscriptionId} activated for customer {CustomerId} with offer {OfferId} starting {StartDate}",
            notification.SubscriptionId,
            notification.CustomerId,
            notification.OfferId,
            notification.StartDate);

        return Task.CompletedTask;
    }
}
