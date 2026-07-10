using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.IntegrationEvents;
using Obss.Billing.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Application.DomainEventHandlers;

public sealed class BalanceChangedEventHandler : INotificationHandler<BalanceChangedEvent>
{
    private readonly IMediator _mediator;
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<BalanceChangedEventHandler> _logger;

    public BalanceChangedEventHandler(
        IMediator mediator,
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        ILogger<BalanceChangedEventHandler> logger)
    {
        _mediator = mediator;
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(BalanceChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Balance changed for billing account {BillingAccountId}: {PreviousBalance} -> {NewBalance} {Currency}.",
            notification.BillingAccountId, notification.PreviousBalance, notification.NewBalance, notification.Currency);

        var integrationEvent = new BillingAccountBalanceChangedIntegrationEvent(
            notification.BillingAccountId,
            notification.PreviousBalance,
            notification.NewBalance,
            notification.Currency)
        {
            TenantId = _currentTenant.TenantId ?? string.Empty,
            CorrelationId = notification.EventId.ToString()
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
        await _mediator.Publish(integrationEvent, cancellationToken);
    }
}
