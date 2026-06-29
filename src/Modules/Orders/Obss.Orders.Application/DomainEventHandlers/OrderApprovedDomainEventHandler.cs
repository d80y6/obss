using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.IntegrationEvents;
using Obss.Orders.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Orders.Application.DomainEventHandlers;

public sealed class OrderApprovedDomainEventHandler : INotificationHandler<OrderApprovedDomainEvent>
{
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly IOrderRepository _orderRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<OrderApprovedDomainEventHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public OrderApprovedDomainEventHandler(
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        IOrderRepository orderRepository,
        IMediator mediator,
        ILogger<OrderApprovedDomainEventHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _orderRepository = orderRepository;
        _mediator = mediator;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(OrderApprovedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order approved: {OrderNumber} (ID: {OrderId}) by {ApprovedBy}",
            notification.OrderNumber,
            notification.OrderId,
            notification.ApprovedBy);

        var tenantId = _currentTenant.TenantId ?? string.Empty;
        var correlationId = notification.EventId.ToString();

        var integrationEvent = new OrderApprovedIntegrationEvent(
            notification.OrderId,
            notification.OrderNumber,
            notification.ApprovedBy,
            notification.OccurredOn)
        {
            TenantId = tenantId,
            CorrelationId = correlationId
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
        await _mediator.Publish(integrationEvent, cancellationToken);

        var order = await _orderRepository.GetByIdWithItemsAsync(notification.OrderId, cancellationToken);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found when processing approval events", notification.OrderId);
            return;
        }

        order.CreateFulfillment();

        foreach (var item in order.Items)
        {
            var serviceType = item.ServiceType ?? "FTTH";

            var provisioningEvent = new ProvisioningRequiredIntegrationEvent(
                order.Id,
                item.Id,
                order.CustomerId,
                serviceType,
                "Provision")
            {
                TenantId = tenantId,
                CorrelationId = correlationId
            };

            await _outboxService.AddAsync(provisioningEvent, cancellationToken);
            await _mediator.Publish(provisioningEvent, cancellationToken);

            var subscriptionEvent = new SubscriptionRequiredIntegrationEvent(
                order.CustomerId,
                order.CustomerName,
                order.Id,
                item.Id,
                item.ProductId,
                item.OfferId,
                item.OfferName,
                item.BillingPeriod.ToString(),
                order.Currency,
                item.UnitPrice,
                item.Quantity)
            {
                TenantId = tenantId,
                CorrelationId = correlationId
            };

            await _outboxService.AddAsync(subscriptionEvent, cancellationToken);
            await _mediator.Publish(subscriptionEvent, cancellationToken);
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
