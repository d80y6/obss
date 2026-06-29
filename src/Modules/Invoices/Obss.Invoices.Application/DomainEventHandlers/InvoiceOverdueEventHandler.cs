using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Domain.Events;

namespace Obss.Invoices.Application.DomainEventHandlers;

public sealed class InvoiceOverdueEventHandler : INotificationHandler<InvoiceOverdueDomainEvent>
{
    private readonly ILogger<InvoiceOverdueEventHandler> _logger;

    public InvoiceOverdueEventHandler(ILogger<InvoiceOverdueEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(InvoiceOverdueDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Invoice {InvoiceNumber} ({InvoiceId}) is overdue by {DaysOverdue} days.",
            notification.InvoiceNumber, notification.InvoiceId, notification.DaysOverdue);

        return Task.CompletedTask;
    }
}
