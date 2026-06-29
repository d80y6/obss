using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Domain.Events;

namespace Obss.Invoices.Application.DomainEventHandlers;

public sealed class InvoicePaidEventHandler : INotificationHandler<InvoicePaidDomainEvent>
{
    private readonly ILogger<InvoicePaidEventHandler> _logger;

    public InvoicePaidEventHandler(ILogger<InvoicePaidEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(InvoicePaidDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Invoice {InvoiceNumber} ({InvoiceId}) paid. Amount: {Amount}, Reference: {Reference}",
            notification.InvoiceNumber, notification.InvoiceId, notification.AmountPaid, notification.PaymentReference);

        return Task.CompletedTask;
    }
}
