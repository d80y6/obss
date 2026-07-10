using MediatR;
using Microsoft.Extensions.Logging;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.IntegrationEvents;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Orders.Application.IntegrationEventHandlers;

public sealed class QuoteAcceptedIntegrationEventHandler : INotificationHandler<QuoteAcceptedIntegrationEvent>
{
    private readonly IQuoteRepository _quoteRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<QuoteAcceptedIntegrationEventHandler> _logger;

    public QuoteAcceptedIntegrationEventHandler(
        IQuoteRepository quoteRepository,
        ICustomerRepository customerRepository,
        IProductOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ILogger<QuoteAcceptedIntegrationEventHandler> logger)
    {
        _quoteRepository = quoteRepository;
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(QuoteAcceptedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating order from accepted quote {QuoteId} for customer {CustomerId}",
            notification.QuoteId,
            notification.CustomerId);

        var quote = await _quoteRepository.GetByIdAsync(notification.QuoteId, cancellationToken);
        if (quote is null)
        {
            _logger.LogWarning("Quote {QuoteId} not found for accepted event", notification.QuoteId);
            return;
        }

        var customer = await _customerRepository.GetByIdAsync(notification.CustomerId, cancellationToken);
        var customerName = customer?.DisplayName ?? "Unknown";

        var tenantId = _currentTenant.TenantId ?? string.Empty;
        var order = ProductOrder.Create(tenantId, quote.CustomerId, customerName, OrderType.New, "system");

        foreach (var quoteItem in quote.Items)
        {
            var productId = quoteItem.ProductId ?? quoteItem.ProductOfferingId ?? Guid.NewGuid();
            order.AddItem(
                productId,
                quoteItem.ProductOfferingId ?? Guid.NewGuid(),
                quoteItem.ProductOfferingName ?? "Quote Item",
                quoteItem.ProductOfferingName ?? "Quote Item",
                quoteItem.Quantity,
                0m,
                0m,
                0m,
                0m,
                BillingPeriod.Monthly);
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} created from accepted quote {QuoteId}",
            order.Id,
            notification.QuoteId);
    }
}
