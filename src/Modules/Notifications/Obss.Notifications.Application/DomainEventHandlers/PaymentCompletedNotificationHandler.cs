using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Notifications.Application.Commands.SendNotification;
using Obss.Payments.Domain.Events;

namespace Obss.Notifications.Application.DomainEventHandlers;

public sealed class PaymentCompletedNotificationHandler : INotificationHandler<PaymentCompletedDomainEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentCompletedNotificationHandler> _logger;

    public PaymentCompletedNotificationHandler(
        IMediator mediator,
        ILogger<PaymentCompletedNotificationHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(PaymentCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling PaymentCompletedDomainEvent for payment {PaymentNumber}",
            notification.PaymentNumber);

        var command = new SendNotificationCommand(
            TenantId: "default",
            NotificationType: "Email",
            Channel: "Email",
            Recipient: $"customer@example.com",
            Subject: $"Payment {notification.PaymentNumber} Completed",
            Body: $"Your payment of {notification.Amount} {notification.Currency} has been completed successfully.",
            Priority: "Normal",
            ReferenceType: "Payment",
            ReferenceId: notification.PaymentId);

        await _mediator.Send(command, cancellationToken);
    }
}
