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
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<SubscriptionRequiredIntegrationEventHandler> _logger;

    public SubscriptionRequiredIntegrationEventHandler(
        ISubscriptionRepository subscriptionRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ILogger<SubscriptionRequiredIntegrationEventHandler> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _productRepository = productRepository;
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

        var product = Product.Create(
            Guid.Parse(tenantId),
            notification.CustomerId,
            notification.OfferName,
            null,
            null,
            notification.OfferId);

        var subscription = Subscription.Create(
            tenantId,
            notification.CustomerId,
            notification.CustomerName,
            notification.OrderId,
            notification.OrderItemId,
            product.Id,
            notification.OfferId,
            notification.OfferName,
            parsedBillingPeriod,
            notification.Currency,
            notification.Price,
            notification.Quantity,
            DateTime.UtcNow,
            null);

        await _productRepository.AddAsync(product, cancellationToken);
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Subscription {SubscriptionId} created from approved order {OrderId} (Product {ProductId})",
            subscription.Id,
            notification.OrderId,
            product.Id);
    }
}
