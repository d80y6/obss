using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Payments.Domain.Events;

namespace Obss.Payments.Application.DomainEventHandlers;

public sealed class PaymentRefundedEventHandler : INotificationHandler<PaymentRefundedDomainEvent>
{
    private readonly ILogger<PaymentRefundedEventHandler> _logger;

    public PaymentRefundedEventHandler(ILogger<PaymentRefundedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PaymentRefundedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Payment {PaymentId} refunded. Amount: {Amount}, Reason: {Reason}",
            notification.PaymentId, notification.Amount, notification.Reason);

        return Task.CompletedTask;
    }
}
