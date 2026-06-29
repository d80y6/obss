using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Payments.Domain.Events;

namespace Obss.Payments.Application.DomainEventHandlers;

public sealed class PaymentCompletedEventHandler : INotificationHandler<PaymentCompletedDomainEvent>
{
    private readonly ILogger<PaymentCompletedEventHandler> _logger;

    public PaymentCompletedEventHandler(ILogger<PaymentCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PaymentCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Payment {PaymentNumber} ({PaymentId}) completed. Amount: {Amount} {Currency}",
            notification.PaymentNumber, notification.PaymentId, notification.Amount, notification.Currency);

        return Task.CompletedTask;
    }
}
