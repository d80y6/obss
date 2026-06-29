using Obss.Rating.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Rating.Application.Abstractions;

public interface IPromotionRepository : IRepository<Promotion>
{
    Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync(CancellationToken cancellationToken = default);
    Task<Promotion?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Promotion>> GetActiveByDateRangeAsync(DateTime date, CancellationToken cancellationToken = default);
}
