using Obss.SharedKernel.Domain.Events;

namespace Obss.Payments.Application.IntegrationEvents;

public sealed class PaymentCompletedIntegrationEvent : IntegrationEvent
{
    public PaymentCompletedIntegrationEvent(
        Guid paymentId,
        string paymentNumber,
        Guid? invoiceId,
        decimal amount,
        string currency,
        string tenantId)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        InvoiceId = invoiceId;
        Amount = amount;
        Currency = currency;
        TenantId = tenantId;
    }

    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public Guid? InvoiceId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
