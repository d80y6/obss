using Microsoft.EntityFrameworkCore;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Billing.Infrastructure.Persistence.Repositories;

public sealed class BillRepository : EfRepository<Bill>, IBillRepository
{
    public BillRepository(BillingDbContext context)
        : base(context)
    {
    }

    public async Task<Bill?> GetByIdWithLinesAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Id == billId, cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetByCustomerAsync(
        Guid? customerId,
        BillStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(b => b.Lines)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(b => b.CustomerId == customerId.Value);

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.CreatedAt <= toDate.Value);
        }

        query = query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetOpenBillsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(b => b.Lines)
            .Where(b => b.Status == BillStatus.Finalized)
            .OrderByDescending(b => b.FinalizedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetBillsDueForGenerationAsync(DateTime upToDate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(b => b.Lines)
            .Where(b => b.Status == BillStatus.Draft && b.CreatedAt <= upToDate)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
