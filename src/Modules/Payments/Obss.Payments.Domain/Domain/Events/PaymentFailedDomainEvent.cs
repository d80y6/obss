using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Payments.Domain.Events;

public sealed class PaymentFailedDomainEvent : DomainEvent, INotification
{
    public PaymentFailedDomainEvent(Guid paymentId, string reason)
    {
        PaymentId = paymentId;
        Reason = reason;
    }

    public Guid PaymentId { get; }
    public string Reason { get; }
}
