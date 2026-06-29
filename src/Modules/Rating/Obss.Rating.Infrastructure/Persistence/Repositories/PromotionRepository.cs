using Microsoft.EntityFrameworkCore;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Rating.Infrastructure.Persistence.Repositories;

public sealed class PromotionRepository : EfRepository<Promotion>, IPromotionRepository
{
    public PromotionRepository(RatingDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Rules)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<Promotion?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(p => p.Code != null && p.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Promotion>> GetActiveByDateRangeAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Rules)
            .Where(p => p.IsActive && p.ValidFrom <= date && (!p.ValidTo.HasValue || p.ValidTo >= date))
            .OrderBy(p => p.Priority)
            .ToListAsync(cancellationToken);
    }
}
