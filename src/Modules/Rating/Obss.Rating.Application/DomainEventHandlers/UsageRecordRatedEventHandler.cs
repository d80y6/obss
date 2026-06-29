using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Rating.Application.DomainEventHandlers;

public sealed class UsageRecordRatedEventHandler : INotificationHandler<UsageRecordRatedDomainEvent>
{
    private readonly IOutboxService _outboxService;
    private readonly ILogger<UsageRecordRatedEventHandler> _logger;

    public UsageRecordRatedEventHandler(IOutboxService outboxService, ILogger<UsageRecordRatedEventHandler> logger)
    {
        _outboxService = outboxService;
        _logger = logger;
    }

    public async Task Handle(UsageRecordRatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UsageRecordRatedIntegrationEvent(
            notification.RecordId,
            notification.SubscriptionId,
            notification.Amount,
            notification.Currency,
            notification.RecordType);

        await _outboxService.AddAsync(integrationEvent, cancellationToken);

        _logger.LogInformation(
            "Usage record {RecordId} rated at {Amount} {Currency} for subscription {SubscriptionId}",
            notification.RecordId,
            notification.Amount,
            notification.Currency,
            notification.SubscriptionId);
    }
}
