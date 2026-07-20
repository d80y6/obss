using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.IntegrationEvents;
using Obss.Billing.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Application.DomainEventHandlers;

public sealed class BillFinalizedEventHandler : INotificationHandler<BillFinalizedDomainEvent>
{
    private readonly IMediator _mediator;
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<BillFinalizedEventHandler> _logger;

    public BillFinalizedEventHandler(
        IMediator mediator,
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        ILogger<BillFinalizedEventHandler> logger)
    {
        _mediator = mediator;
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(BillFinalizedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Bill {BillId} finalized for customer {CustomerId}. Grand total: {GrandTotal} {Currency}. Creating invoice.",
            notification.BillId,
            notification.CustomerId,
            notification.GrandTotal,
            notification.Currency);

        var integrationEvent = new BillFinalizedIntegrationEvent(
            notification.BillId,
            notification.CustomerId,
            notification.GrandTotal,
            notification.Currency)
        {
            TenantId = _currentTenant.TenantId ?? string.Empty,
            CorrelationId = notification.EventId.ToString()
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
        await _mediator.Publish(integrationEvent, cancellationToken);
    }
}
