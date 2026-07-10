using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.IntegrationEvents;
using Obss.Billing.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Application.DomainEventHandlers;

public sealed class BillingAccountDeletedEventHandler : INotificationHandler<BillingAccountDeletedEvent>
{
    private readonly IMediator _mediator;
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<BillingAccountDeletedEventHandler> _logger;

    public BillingAccountDeletedEventHandler(
        IMediator mediator,
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        ILogger<BillingAccountDeletedEventHandler> logger)
    {
        _mediator = mediator;
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(BillingAccountDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Billing account {BillingAccountId} deleted.",
            notification.BillingAccountId);

        var integrationEvent = new BillingAccountDeletedIntegrationEvent(notification.BillingAccountId)
        {
            TenantId = _currentTenant.TenantId ?? string.Empty,
            CorrelationId = notification.EventId.ToString()
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
        await _mediator.Publish(integrationEvent, cancellationToken);
    }
}
