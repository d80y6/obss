using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.IntegrationEvents;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Orders.Application.IntegrationEventHandlers;

public sealed class OrderApprovedIntegrationEventHandler : INotificationHandler<ProductOrderApprovedIntegrationEvent>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderApprovedIntegrationEventHandler> _logger;

    public OrderApprovedIntegrationEventHandler(
        IProductOrderRepository orderRepository,
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        IUnitOfWork unitOfWork,
        ILogger<OrderApprovedIntegrationEventHandler> logger)
    {
        _orderRepository = orderRepository;
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ProductOrderApprovedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing fulfillment for approved order {OrderId}",
            notification.OrderId);

        var order = await _orderRepository.GetByIdWithItemsAsync(notification.OrderId, cancellationToken);
        if (order?.Fulfillment is not null)
        {
            var fulfillment = order.Fulfillment;
            order.StartFulfillment();
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var integrationEvent = new OrderFulfillmentStartedIntegrationEvent(
                notification.OrderId,
                fulfillment.Id,
                fulfillment.Status.ToString())
            {
                TenantId = _currentTenant.TenantId ?? string.Empty,
                CorrelationId = notification.CorrelationId
            };

            await _outboxService.AddAsync(integrationEvent, cancellationToken);

            _logger.LogInformation(
                "Fulfillment {FulfillmentId} started for order {OrderId}",
                fulfillment.Id,
                notification.OrderId);
        }
    }
}
