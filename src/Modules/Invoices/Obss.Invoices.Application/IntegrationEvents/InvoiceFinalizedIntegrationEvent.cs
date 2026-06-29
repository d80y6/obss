using Obss.SharedKernel.Domain.Events;

namespace Obss.Invoices.Application.IntegrationEvents;

public sealed class InvoiceFinalizedIntegrationEvent : IntegrationEvent
{
    public InvoiceFinalizedIntegrationEvent(
        Guid invoiceId,
        string invoiceNumber,
        Guid customerId,
        decimal grandTotal,
        string tenantId)
    {
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        CustomerId = customerId;
        GrandTotal = grandTotal;
        TenantId = tenantId;
    }

    public Guid InvoiceId { get; }
    public string InvoiceNumber { get; }
    public Guid CustomerId { get; }
    public decimal GrandTotal { get; }
}
