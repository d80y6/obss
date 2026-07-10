using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.IntegrationEvents;
using Obss.Billing.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Application.DomainEventHandlers;

public sealed class BillingAccountCreatedEventHandler : INotificationHandler<BillingAccountCreatedEvent>
{
    private readonly IMediator _mediator;
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<BillingAccountCreatedEventHandler> _logger;

    public BillingAccountCreatedEventHandler(
        IMediator mediator,
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        ILogger<BillingAccountCreatedEventHandler> logger)
    {
        _mediator = mediator;
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(BillingAccountCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Billing account {BillingAccountId} created for customer {CustomerId}. Type: {AccountType}.",
            notification.BillingAccountId, notification.CustomerId, notification.AccountType);

        var integrationEvent = new BillingAccountCreatedIntegrationEvent(
            notification.BillingAccountId,
            notification.CustomerId,
            notification.AccountType)
        {
            TenantId = _currentTenant.TenantId ?? string.Empty,
            CorrelationId = notification.EventId.ToString()
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
        await _mediator.Publish(integrationEvent, cancellationToken);
    }
}
