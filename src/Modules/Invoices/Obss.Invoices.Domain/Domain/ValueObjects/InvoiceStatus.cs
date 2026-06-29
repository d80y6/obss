namespace Obss.Invoices.Domain.ValueObjects;

public enum InvoiceStatus
{
    Draft,
    Finalized,
    Sent,
    Paid,
    Overdue,
    Cancelled,
    Void
}
