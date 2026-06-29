using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Collections.Domain.Events;

namespace Obss.Collections.Application.DomainEventHandlers;

public sealed class PaymentArrangementCreatedEventHandler : INotificationHandler<PaymentArrangementCreatedDomainEvent>
{
    private readonly ILogger<PaymentArrangementCreatedEventHandler> _logger;

    public PaymentArrangementCreatedEventHandler(ILogger<PaymentArrangementCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PaymentArrangementCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Payment arrangement {ArrangementId} created for case {CaseId}, customer {CustomerId}. Total: {Amount}, {Count} installments.",
            notification.ArrangementId,
            notification.CollectionCaseId,
            notification.CustomerId,
            notification.TotalAmount,
            notification.InstallmentCount);

        return Task.CompletedTask;
    }
}
