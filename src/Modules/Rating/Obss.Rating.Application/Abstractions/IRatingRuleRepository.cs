using Obss.Rating.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Rating.Application.Abstractions;

public interface IRatingRuleRepository : IRepository<RatingRule>
{
    Task<IReadOnlyList<RatingRule>> GetActiveRulesOrderedByPriorityAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RatingRule>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<RatingRule?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
