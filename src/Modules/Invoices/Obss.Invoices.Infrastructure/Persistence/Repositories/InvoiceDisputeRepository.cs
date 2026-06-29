using Microsoft.EntityFrameworkCore;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Invoices.Infrastructure.Persistence.Repositories;

public sealed class InvoiceDisputeRepository : EfRepository<InvoiceDispute>, IInvoiceDisputeRepository
{
    public InvoiceDisputeRepository(InvoiceDbContext context)
        : base(context)
    {
    }

    public async Task<InvoiceDispute?> GetByIdWithAttachmentsAsync(Guid disputeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Attachments)
            .FirstOrDefaultAsync(d => d.Id == disputeId, cancellationToken);
    }

    public async Task<IReadOnlyList<InvoiceDispute>> GetDisputesAsync(
        Guid? invoiceId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(d => d.Attachments)
            .AsQueryable();

        if (invoiceId.HasValue)
        {
            query = query.Where(d => d.InvoiceId == invoiceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DisputeStatus>(status, ignoreCase: true, out var disputeStatus))
        {
            query = query.Where(d => d.Status == disputeStatus);
        }

        query = query.OrderByDescending(d => d.CreatedAt);

        return await query.ToListAsync(cancellationToken);
    }
}
