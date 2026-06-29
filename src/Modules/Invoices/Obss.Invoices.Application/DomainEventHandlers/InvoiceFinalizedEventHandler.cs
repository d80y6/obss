using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Domain.Events;

namespace Obss.Invoices.Application.DomainEventHandlers;

public sealed class InvoiceFinalizedEventHandler : INotificationHandler<InvoiceFinalizedDomainEvent>
{
    private readonly ILogger<InvoiceFinalizedEventHandler> _logger;

    public InvoiceFinalizedEventHandler(ILogger<InvoiceFinalizedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(InvoiceFinalizedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Invoice {InvoiceNumber} ({InvoiceId}) finalized. Grand total: {GrandTotal}",
            notification.InvoiceNumber, notification.InvoiceId, notification.GrandTotal);

        return Task.CompletedTask;
    }
}
