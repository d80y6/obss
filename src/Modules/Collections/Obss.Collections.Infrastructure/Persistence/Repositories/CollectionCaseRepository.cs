using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Domain.Entities;
using Obss.Collections.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Collections.Infrastructure.Persistence.Repositories;

public sealed class CollectionCaseRepository : EfRepository<CollectionCase>, ICollectionCaseRepository
{
    public CollectionCaseRepository(CollectionDbContext context)
        : base(context)
    {
    }

    public override async Task<IReadOnlyList<CollectionCase>> FindAsync(Expression<Func<CollectionCase, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Actions)
            .Include(c => c.PaymentArrangements)
            .Where(predicate)
            .OrderByDescending(c => c.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CollectionCase>> FindPagedAsync(Expression<Func<CollectionCase, bool>> predicate, int offset, int limit, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Actions)
            .Include(c => c.PaymentArrangements)
            .Where(predicate)
            .OrderByDescending(c => c.OpenedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<CollectionCase?> GetByIdWithDetailsAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Actions)
            .Include(c => c.PaymentArrangements)
                .ThenInclude(pa => pa.Installments)
            .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);
    }

    public async Task<IReadOnlyList<CollectionCase>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Actions)
            .Include(c => c.PaymentArrangements)
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CollectionCase>> GetByStatusAsync(CollectionCaseStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Actions)
            .Include(c => c.PaymentArrangements)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.LastActionAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CollectionCase>> GetByDunningLevelAsync(int level, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Actions)
            .Include(c => c.PaymentArrangements)
            .Where(c => c.CurrentDunningLevel == level && c.Status != CollectionCaseStatus.Closed && c.Status != CollectionCaseStatus.Resolved)
            .OrderByDescending(c => c.LastActionAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CollectionCase>> GetActiveCasesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Actions)
            .Include(c => c.PaymentArrangements)
                .ThenInclude(pa => pa.Installments)
            .Where(c => c.Status == CollectionCaseStatus.Open || c.Status == CollectionCaseStatus.InProgress)
            .OrderByDescending(c => c.LastActionAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CollectionCase?> GetByCustomerWithActiveArrangementAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.PaymentArrangements)
            .Include(c => c.Actions)
            .Where(c => c.CustomerId == customerId &&
                        (c.Status == CollectionCaseStatus.Open ||
                         c.Status == CollectionCaseStatus.InProgress))
            .OrderByDescending(c => c.OpenedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<int, (int CaseCount, int CustomerCount, decimal TotalAmount)>> GetAgingBucketsAsync(string currency, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var cases = await DbSet
            .Where(c => c.Currency == currency && c.Status != CollectionCaseStatus.Closed && c.Status != CollectionCaseStatus.Resolved)
            .Select(c => new
            {
                LastActionOrOpened = c.LastActionAt ?? c.OpenedAt,
                c.TotalOverdueAmount,
                c.CustomerId
            })
            .ToListAsync(cancellationToken);

        return cases
            .GroupBy(c => (int)(now - c.LastActionOrOpened).TotalDays)
            .ToDictionary(
                g => g.Key < 0 ? 0 : g.Key,
                g => (g.Count(), g.Select(c => c.CustomerId).Distinct().Count(), g.Sum(c => c.TotalOverdueAmount)));
    }
}
