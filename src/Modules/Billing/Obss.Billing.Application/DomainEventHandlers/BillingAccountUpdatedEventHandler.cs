using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.IntegrationEvents;
using Obss.Billing.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Application.DomainEventHandlers;

public sealed class BillingAccountUpdatedEventHandler : INotificationHandler<BillingAccountUpdatedEvent>
{
    private readonly IMediator _mediator;
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<BillingAccountUpdatedEventHandler> _logger;

    public BillingAccountUpdatedEventHandler(
        IMediator mediator,
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        ILogger<BillingAccountUpdatedEventHandler> logger)
    {
        _mediator = mediator;
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(BillingAccountUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Billing account {BillingAccountId} updated. Name: {Name}, Status: {Status}.",
            notification.BillingAccountId, notification.Name, notification.Status);

        var integrationEvent = new BillingAccountUpdatedIntegrationEvent(
            notification.BillingAccountId,
            notification.Name,
            notification.Status)
        {
            TenantId = _currentTenant.TenantId ?? string.Empty,
            CorrelationId = notification.EventId.ToString()
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
        await _mediator.Publish(integrationEvent, cancellationToken);
    }
}
