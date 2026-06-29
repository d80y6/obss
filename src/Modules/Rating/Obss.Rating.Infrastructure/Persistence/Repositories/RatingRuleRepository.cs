using Microsoft.EntityFrameworkCore;
using Obss.Rating.Application.Abstractions;
using Obss.Rating.Domain.Entities;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Rating.Infrastructure.Persistence.Repositories;

public sealed class RatingRuleRepository : EfRepository<RatingRule>, IRatingRuleRepository
{
    public RatingRuleRepository(RatingDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<RatingRule>> GetActiveRulesOrderedByPriorityAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RatingRule>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<RatingRule?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }
}
