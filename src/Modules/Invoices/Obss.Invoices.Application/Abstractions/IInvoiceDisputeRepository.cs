using Obss.Invoices.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Invoices.Application.Abstractions;

public interface IInvoiceDisputeRepository : IRepository<InvoiceDispute>
{
    Task<InvoiceDispute?> GetByIdWithAttachmentsAsync(Guid disputeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvoiceDispute>> GetDisputesAsync(Guid? invoiceId, string? status, CancellationToken cancellationToken = default);
}
