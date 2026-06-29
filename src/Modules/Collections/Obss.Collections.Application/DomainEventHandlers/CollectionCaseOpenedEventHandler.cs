using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Collections.Domain.Events;

namespace Obss.Collections.Application.DomainEventHandlers;

public sealed class CollectionCaseOpenedEventHandler : INotificationHandler<CollectionCaseOpenedDomainEvent>
{
    private readonly ILogger<CollectionCaseOpenedEventHandler> _logger;

    public CollectionCaseOpenedEventHandler(ILogger<CollectionCaseOpenedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CollectionCaseOpenedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Collection case {CaseId} opened for customer {CustomerId} ({CustomerName}). Overdue amount: {Amount} {Currency}.",
            notification.CaseId,
            notification.CustomerId,
            notification.CustomerName,
            notification.TotalOverdueAmount,
            notification.Currency);

        return Task.CompletedTask;
    }
}
