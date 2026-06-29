using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Orders.Application.IntegrationEvents;
using Obss.SharedKernel.Application.Abstractions;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Domain.Entities;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.IntegrationEventHandlers;

public sealed class SubscriptionRequiredIntegrationEventHandler : INotificationHandler<SubscriptionRequiredIntegrationEvent>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<SubscriptionRequiredIntegrationEventHandler> _logger;

    public SubscriptionRequiredIntegrationEventHandler(
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ILogger<SubscriptionRequiredIntegrationEventHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(SubscriptionRequiredIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId ?? string.Empty;

        if (!Enum.TryParse<BillingPeriod>(notification.BillingPeriod, true, out var parsedBillingPeriod))
        {
            _logger.LogError("Invalid billing period '{BillingPeriod}' for order {OrderId}", notification.BillingPeriod, notification.OrderId);
            return;
        }

        var subscription = Subscription.Create(
            tenantId,
            notification.CustomerId,
            notification.CustomerName,
            notification.OrderId,
            notification.OrderItemId,
            notification.ProductId,
            notification.OfferId,
            notification.OfferName,
            parsedBillingPeriod,
            notification.Currency,
            notification.Price,
            notification.Quantity,
            DateTime.UtcNow,
            null);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Subscription {SubscriptionId} created from approved order {OrderId}",
            subscription.Id,
            notification.OrderId);
    }
}
