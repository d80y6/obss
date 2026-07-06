using Microsoft.EntityFrameworkCore;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Payments.Infrastructure.Persistence.Repositories;

public sealed class PaymentReconciliationRepository : EfRepository<PaymentReconciliation>, IPaymentReconciliationRepository
{
    public PaymentReconciliationRepository(PaymentDbContext context)
        : base(context)
    {
    }

    public async Task<PaymentReconciliation?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentReconciliation>> GetByStatusAsync(string status, string tenantId, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ReconciliationStatus>(status, true, out var reconciliationStatus))
            return [];

        return await DbSet
            .Include(r => r.Items)
            .Where(r => r.TenantId == tenantId && r.Status == reconciliationStatus)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentReconciliation>> GetFilteredAsync(
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        int offset = 0,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(r => r.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReconciliationStatus>(status, true, out var reconciliationStatus))
            query = query.Where(r => r.Status == reconciliationStatus);

        if (fromDate.HasValue)
            query = query.Where(r => r.ImportDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(r => r.ImportDate <= toDate.Value);

        query = query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReconciliationItem>> GetUnmatchedItemsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.TenantId == tenantId)
            .SelectMany(r => r.Items)
            .Where(i => i.Status == ReconciliationItemStatus.Unmatched)
            .ToListAsync(cancellationToken);
    }
}
