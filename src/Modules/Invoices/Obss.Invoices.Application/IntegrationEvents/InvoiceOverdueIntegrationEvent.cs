using Obss.SharedKernel.Domain.Events;

namespace Obss.Invoices.Application.IntegrationEvents;

public sealed class InvoiceOverdueIntegrationEvent : IntegrationEvent
{
    public InvoiceOverdueIntegrationEvent(
        Guid invoiceId,
        string invoiceNumber,
        int daysOverdue,
        string tenantId)
    {
        InvoiceId = invoiceId;
        InvoiceNumber = invoiceNumber;
        DaysOverdue = daysOverdue;
        TenantId = tenantId;
    }

    public Guid InvoiceId { get; }
    public string InvoiceNumber { get; }
    public int DaysOverdue { get; }
}
