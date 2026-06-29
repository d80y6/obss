using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Domain.Events;
using Obss.Notifications.Application.Commands.SendNotification;

namespace Obss.Notifications.Application.DomainEventHandlers;

public sealed class InvoiceFinalizedNotificationHandler : INotificationHandler<InvoiceFinalizedDomainEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<InvoiceFinalizedNotificationHandler> _logger;

    public InvoiceFinalizedNotificationHandler(
        IMediator mediator,
        ILogger<InvoiceFinalizedNotificationHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(InvoiceFinalizedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling InvoiceFinalizedDomainEvent for invoice {InvoiceNumber}",
            notification.InvoiceNumber);

        var command = new SendNotificationCommand(
            TenantId: "default",
            NotificationType: "Email",
            Channel: "Email",
            Recipient: $"customer-{notification.CustomerId}@example.com",
            Subject: $"Invoice {notification.InvoiceNumber} Finalized",
            Body: $"Your invoice {notification.InvoiceNumber} has been finalized. Total amount: {notification.GrandTotal:C}.",
            Priority: "Normal",
            ReferenceType: "Invoice",
            ReferenceId: notification.InvoiceId);

        await _mediator.Send(command, cancellationToken);
    }
}
