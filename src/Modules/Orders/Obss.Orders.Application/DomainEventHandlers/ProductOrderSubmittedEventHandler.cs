using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Orders.Application.IntegrationEvents;
using Obss.Orders.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Orders.Application.DomainEventHandlers;

public sealed class ProductOrderSubmittedEventHandler : INotificationHandler<ProductOrderSubmittedDomainEvent>
{
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<ProductOrderSubmittedEventHandler> _logger;

    public ProductOrderSubmittedEventHandler(
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        ILogger<ProductOrderSubmittedEventHandler> logger)
    {
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(ProductOrderSubmittedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order submitted: {OrderNumber} (ID: {OrderId}) by customer {CustomerId}",
            notification.OrderNumber,
            notification.OrderId,
            notification.CustomerId);

        var integrationEvent = new ProductOrderSubmittedIntegrationEvent(
            notification.OrderId,
            notification.OrderNumber,
            notification.CustomerId,
            notification.GrandTotal,
            notification.Currency,
            notification.OrderItems.Select(i => new ProductOrderItemIntegrationData(
                i.ProductId,
                i.OfferId,
                i.ProductName,
                i.OfferName,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice)).ToList())
        {
            TenantId = _currentTenant.TenantId ?? string.Empty,
            CorrelationId = notification.EventId.ToString()
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
    }
}
