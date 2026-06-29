using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Payments.Domain.Events;

public sealed class PaymentRefundedDomainEvent : DomainEvent, INotification
{
    public PaymentRefundedDomainEvent(Guid paymentId, decimal amount, string reason)
    {
        PaymentId = paymentId;
        Amount = amount;
        Reason = reason;
    }

    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public string Reason { get; }
}
