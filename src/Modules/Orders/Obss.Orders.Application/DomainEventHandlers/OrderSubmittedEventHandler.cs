using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Orders.Application.IntegrationEvents;
using Obss.Orders.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Orders.Application.DomainEventHandlers;

public sealed class OrderSubmittedEventHandler : INotificationHandler<OrderSubmittedDomainEvent>
{
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<OrderSubmittedEventHandler> _logger;

    public OrderSubmittedEventHandler(
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        ILogger<OrderSubmittedEventHandler> logger)
    {
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(OrderSubmittedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order submitted: {OrderNumber} (ID: {OrderId}) by customer {CustomerId}",
            notification.OrderNumber,
            notification.OrderId,
            notification.CustomerId);

        var integrationEvent = new OrderSubmittedIntegrationEvent(
            notification.OrderId,
            notification.OrderNumber,
            notification.CustomerId,
            notification.GrandTotal,
            notification.Currency,
            notification.OrderItems.Select(i => new OrderItemIntegrationData(
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
