using Obss.ServiceCatalog.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceCatalog.Application.Abstractions;

public interface IServiceCandidateRepository : IRepository<ServiceCandidate>
{
    Task<List<ServiceCandidate>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<ServiceCandidate?> GetByIdWithCategoriesAsync(Guid id, CancellationToken ct = default);
}
