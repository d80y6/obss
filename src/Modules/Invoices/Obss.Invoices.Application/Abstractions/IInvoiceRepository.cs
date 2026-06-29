using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Invoices.Application.Abstractions;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetByIdWithDetailsAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> GetByCustomerAsync(
        Guid customerId,
        InvoiceStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default);
    Task<InvoiceSummary> GetInvoiceSummaryAsync(CancellationToken cancellationToken = default);
    Task<string> GenerateNextInvoiceNumberAsync(CancellationToken cancellationToken = default);
    Task<CreditNote?> GetCreditNoteByIdAsync(Guid creditNoteId, CancellationToken cancellationToken = default);
    Task<CreditNote> AddCreditNoteAsync(CreditNote creditNote, CancellationToken cancellationToken = default);
}

public sealed record InvoiceSummary(
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
