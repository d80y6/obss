using Microsoft.EntityFrameworkCore;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Domain.Entities;
using Obss.Invoices.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Invoices.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository : EfRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(InvoiceDbContext context)
        : base(context)
    {
    }

    public async Task<Invoice?> GetByIdWithDetailsAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .Include(i => i.NotesCollection)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetByCustomerAsync(
        Guid customerId,
        InvoiceStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .AsQueryable();

        query = query.Where(i => i.CustomerId == customerId);

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate <= toDate.Value);
        }

        query = query.OrderByDescending(i => i.InvoiceDate);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await DbSet
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < now)
            .OrderBy(i => i.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<InvoiceSummary> GetInvoiceSummaryAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await DbSet.ToListAsync(cancellationToken);

        return new InvoiceSummary(
            invoices.Count,
            invoices.Count(i => i.Status == InvoiceStatus.Draft),
            invoices.Count(i => i.Status == InvoiceStatus.Finalized),
            invoices.Count(i => i.Status == InvoiceStatus.Sent),
            invoices.Count(i => i.Status == InvoiceStatus.Paid),
            invoices.Count(i => i.Status == InvoiceStatus.Overdue),
            invoices.Count(i => i.Status == InvoiceStatus.Cancelled),
            invoices.Count(i => i.Status == InvoiceStatus.Void),
            invoices.Sum(i => i.GrandTotal),
            invoices.Sum(i => i.AmountPaid),
            invoices.Sum(i => i.AmountDue));
    }

    public async Task<string> GenerateNextInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        var lastInvoice = await DbSet
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastInvoice is null)
        {
            return $"{prefix}00001";
        }

        var lastNumber = lastInvoice.InvoiceNumber[prefix.Length..];
        if (int.TryParse(lastNumber, out var sequence))
        {
            return $"{prefix}{sequence + 1:D5}";
        }

        return $"{prefix}00001";
    }

    public async Task<CreditNote?> GetCreditNoteByIdAsync(Guid creditNoteId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CreditNote>()
            .Include(cn => cn.Lines)
            .FirstOrDefaultAsync(cn => cn.Id == creditNoteId, cancellationToken);
    }

    public async Task<CreditNote> AddCreditNoteAsync(CreditNote creditNote, CancellationToken cancellationToken = default)
    {
        var entry = await Context.Set<CreditNote>().AddAsync(creditNote, cancellationToken);
        return entry.Entity;
    }
}
