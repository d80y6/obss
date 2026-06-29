using MediatR;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Payments.Domain.Events;

public sealed class PaymentCompletedDomainEvent : DomainEvent, INotification
{
    public PaymentCompletedDomainEvent(Guid paymentId, string paymentNumber, Guid? invoiceId, decimal amount, string currency)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        InvoiceId = invoiceId;
        Amount = amount;
        Currency = currency;
    }

    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public Guid? InvoiceId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
