using Obss.SharedKernel.Domain.Events;

namespace Obss.Invoices.Application.IntegrationEvents;

public sealed class InvoicePaidIntegrationEvent : IntegrationEvent
{
    public InvoicePaidIntegrationEvent(
        Guid invoiceId,
        string invoiceNumber,
        decimal amountPaid,
        string paymentReference,
        string tenantId)
    {
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        AmountPaid = amountPaid;
        PaymentReference = paymentReference;
        TenantId = tenantId;
    }

    public Guid InvoiceId { get; }
    public string InvoiceNumber { get; }
    public decimal AmountPaid { get; }
    public string PaymentReference { get; }
}
