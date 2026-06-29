namespace Obss.Invoices.Application.DTOs;

public sealed record InvoiceSummaryDto(
    int TotalInvoices,
    int DraftCount,
    int FinalizedCount,
    int SentCount,
    int PaidCount,
    int OverdueCount,
    int CancelledCount,
    int VoidCount,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal TotalOutstanding);
